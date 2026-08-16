using RentalBlazorApp.Models.AI;
using RentalBlazorApp.Services.AI.Interfaces;
using System.Collections.Concurrent;

namespace RentalBlazorApp.Services.AI;

/// <summary>
/// Orchestrates the full chat pipeline:
/// Receive user message → Update ConversationContext → Build Prompt → Call Groq → Save response → Return ChatResponse.
///
/// ConversationContext is stored in a thread-safe ConcurrentDictionary so that every HTTP
/// request within the same session can access the same conversation history.
/// </summary>
public sealed class ChatService : IChatService
{
    // ─────────────────────────────────────────────────────────────────────────
    // Dependencies
    // ─────────────────────────────────────────────────────────────────────────

    private readonly IGroqService _groqService;
    private readonly IPromptService _promptService;
    private readonly ILogger<ChatService> _logger;

    // ─────────────────────────────────────────────────────────────────────────
    // In-memory conversation store  (sessionId → ConversationContext)
    //
    // WHY ConcurrentDictionary?
    //   ChatService is registered as Scoped (one instance per HTTP request).
    //   But the store itself must survive across requests for the same session.
    //   We keep it as a *static* field so it lives for the application lifetime,
    //   while still being thread-safe under concurrent Blazor Server connections.
    //
    // LIMITATION: Data is lost on app restart. For persistence, replace with a
    // distributed cache (Redis) or a DB table.
    // ─────────────────────────────────────────────────────────────────────────

    private static readonly ConcurrentDictionary<string, ConversationContext> _sessions = new();

    // Maximum messages kept per session to stay within Groq's context window.
    private const int MaxHistoryMessages = 20;

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────

    public ChatService(
        IGroqService groqService,
        IPromptService promptService,
        ILogger<ChatService> logger)
    {
        _groqService   = groqService   ?? throw new ArgumentNullException(nameof(groqService));
        _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProcessMessageAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Main orchestration method.
    ///
    /// Flow:
    ///   1. Validate the incoming ChatRequest.
    ///   2. Retrieve or create the ConversationContext for the session.
    ///   3. Build the full GroqRequest via PromptService (system prompt + history + new message).
    ///   4. Send the request to the Groq API via GroqService.
    ///   5. Extract the assistant reply from GroqResponse.
    ///   6. Persist both the user message and the assistant reply into the context.
    ///   7. Return a ChatResponse to the caller.
    /// </summary>
    public async Task<ChatResponse> ProcessMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        
        if (request is null)
        {
            _logger.LogWarning("ProcessMessageAsync received a null ChatRequest.");
            return BuildErrorResponse("Request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            _logger.LogWarning("ProcessMessageAsync received an empty SessionId.");
            return BuildErrorResponse("SessionId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            _logger.LogWarning("ProcessMessageAsync received an empty message for session {SessionId}.", request.SessionId);
            return BuildErrorResponse("Message cannot be empty.");
        }

        _logger.LogInformation(
            "Processing message for session {SessionId}. Message length: {Length} chars.",
            request.SessionId,
            request.Message.Length);

        try
        {
            // ── Step 2: Retrieve or create ConversationContext ────────────────
            var context = _sessions.GetOrAdd(request.SessionId, sessionId => new ConversationContext
            {
                SessionId   = sessionId,
                UserId      = request.UserId,
                StartedAt   = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            });

            // Update last activity timestamp.
            context.LastActivityAt = DateTime.UtcNow;

            // ── Step 3: Build the GroqRequest (system prompt + history + new msg)
            var groqRequest = await _promptService.BuildPromptAsync(context, request.Message);

            // ── Step 4: Send to Groq API ──────────────────────────────────────
            var groqResponse = await _groqService.SendAsync(groqRequest, cancellationToken);

            // ── Step 5: Validate the Groq response ───────────────────────────
            if (groqResponse is null)
            {
                _logger.LogError(
                    "Groq API returned null for session {SessionId}.", request.SessionId);
                return BuildErrorResponse("The AI service did not return a response. Please try again.");
            }

            if (groqResponse.Choices is null || groqResponse.Choices.Count == 0)
            {
                _logger.LogError(
                    "Groq API returned an empty Choices list for session {SessionId}.", request.SessionId);
                return BuildErrorResponse("The AI service returned an empty response.");
            }

            var assistantContent = groqResponse.Choices[0].Message?.Content;

            if (string.IsNullOrWhiteSpace(assistantContent))
            {
                _logger.LogError(
                    "Groq API returned a choice with null/empty content for session {SessionId}.", request.SessionId);
                return BuildErrorResponse("The AI service returned an unreadable response.");
            }

            // ── Step 6: Persist messages into ConversationContext ─────────────
            // Save user message.
            context.Messages.Add(new ChatMessage
            {
                Role      = "user",
                Content   = request.Message,
                Timestamp = DateTime.UtcNow
            });

            // Save assistant reply.
            context.Messages.Add(new ChatMessage
            {
                Role      = "assistant",
                Content   = assistantContent,
                Timestamp = DateTime.UtcNow
            });

            // Trim history to prevent unbounded memory growth.
            TrimHistory(context);

            _logger.LogInformation(
                "Received response for session {SessionId}. Tokens used: {Tokens}. History size: {Count}.",
                request.SessionId,
                groqResponse.Usage?.TotalTokens,
                context.Messages.Count);

            // ── Step 7: Return success ChatResponse ───────────────────────────
            return new ChatResponse
            {
                Message         = assistantContent,
                IsSuccess       = true,
                ErrorMessage    = null,
                Timestamp       = DateTime.UtcNow,
                TotalTokensUsed = groqResponse.Usage?.TotalTokens
            };
        }
        catch (OperationCanceledException)
        {
            // Request was cancelled (e.g. browser navigated away).
            _logger.LogWarning("ProcessMessageAsync was cancelled for session {SessionId}.", request.SessionId);
            return BuildErrorResponse("The request was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in ProcessMessageAsync for session {SessionId}.",
                request.SessionId);
            return BuildErrorResponse("An unexpected error occurred. Please try again later.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ResetConversationAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Clears the stored ConversationContext for the given session,
    /// allowing the user to start a fresh conversation.
    /// </summary>
    public Task ResetConversationAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            _logger.LogWarning("ResetConversationAsync called with an empty sessionId.");
            return Task.CompletedTask;
        }

        if (_sessions.TryRemove(sessionId, out _))
        {
            _logger.LogInformation("Conversation context cleared for session {SessionId}.", sessionId);
        }
        else
        {
            _logger.LogInformation(
                "ResetConversationAsync: No active session found for {SessionId} — nothing to clear.", sessionId);
        }

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Keeps the conversation history below MaxHistoryMessages by removing
    /// the oldest messages first (sliding window).
    /// </summary>
    private static void TrimHistory(ConversationContext context)
    {
        while (context.Messages.Count > MaxHistoryMessages)
        {
            context.Messages.RemoveAt(0);
        }
    }

    /// <summary>
    /// Convenience factory for a failed ChatResponse.
    /// Avoids repeating the same object initialiser in every error branch.
    /// </summary>
    private static ChatResponse BuildErrorResponse(string errorMessage) => new()
    {
        Message      = null,
        IsSuccess    = false,
        ErrorMessage = errorMessage,
        Timestamp    = DateTime.UtcNow
    };
}
