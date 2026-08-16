using Microsoft.AspNetCore.Mvc;
using RentalBlazorApp.Models.AI;
using RentalBlazorApp.Services.AI.Interfaces;

namespace RentalBlazorApp.Controllers;

/// <summary>
/// REST API controller that exposes the AI Rental Assistant to clients.
///
/// Base route: /api/chat
///
/// Endpoints:
///   POST   /api/chat/message          → Send a user message, receive an AI reply.
///   DELETE /api/chat/session/{id}     → Clear the conversation history for a session.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ChatController : ControllerBase
{
   
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

  
    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/chat/message
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Accepts a user message and returns the AI assistant's reply.
    ///
    /// HTTP 200 OK           – Assistant replied successfully.
    /// HTTP 400 Bad Request  – Validation failure (empty message, missing session ID).
    /// HTTP 503 Unavailable  – Groq API failed or returned an empty response.
    /// HTTP 500 Internal     – Unexpected server-side error.
    ///
    /// Example request body:
    /// {
    ///   "sessionId": "abc-123",
    ///   "message":   "What cars do you have available?",
    ///   "userId":    "user-guid-optional"
    /// }
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        // ── Guard: model binder validation (covers [Required] attributes) ─────
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("SendMessage: invalid model state.");
            return BadRequest(ModelState);
        }

        // ── Guard: manual null checks for required fields ─────────────────────
        if (request is null)
        {
            _logger.LogWarning("SendMessage: request body is null.");
            return BadRequest(new { error = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            _logger.LogWarning("SendMessage: SessionId is missing.");
            return BadRequest(new { error = "SessionId is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            _logger.LogWarning("SendMessage: Message is empty for session {SessionId}.", request.SessionId);
            return BadRequest(new { error = "Message cannot be empty." });
        }

        _logger.LogInformation(
            "SendMessage called. Session: {SessionId}, MessageLength: {Len}.",
            request.SessionId,
            request.Message.Length);

        try
        {
            var response = await _chatService.ProcessMessageAsync(request, cancellationToken);

            // ── Translate the service result into an HTTP response ─────────────
            if (response.IsSuccess)
            {
                // 200 OK – everything went well.
                return Ok(response);
            }

            // Service returned IsSuccess = false — determine appropriate status.
            // "cancelled" maps to 400; Groq failure maps to 503.
            if (response.ErrorMessage?.Contains("cancelled", StringComparison.OrdinalIgnoreCase) == true)
            {
                return BadRequest(new { error = response.ErrorMessage });
            }

            // Groq API unavailable / returned bad data.
            _logger.LogWarning(
                "SendMessage: ChatService returned failure for session {SessionId}. Reason: {Reason}.",
                request.SessionId,
                response.ErrorMessage);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = response.ErrorMessage ?? "The AI service is temporarily unavailable."
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "SendMessage was cancelled by the client for session {SessionId}.", request.SessionId);
            return StatusCode(StatusCodes.Status499ClientClosedRequest,
                new { error = "Request was cancelled by the client." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception in SendMessage for session {SessionId}.", request.SessionId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred. Please try again later." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/chat/session/{sessionId}
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Clears the conversation history for the given session.
    ///
    /// HTTP 200 OK           – Session cleared (or did not exist — idempotent).
    /// HTTP 400 Bad Request  – sessionId is missing or whitespace.
    /// HTTP 500 Internal     – Unexpected error.
    /// </summary>
    [HttpDelete("session/{sessionId}")]
    public async Task<IActionResult> ResetSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            _logger.LogWarning("ResetSession called with an empty sessionId.");
            return BadRequest(new { error = "sessionId route parameter is required." });
        }

        _logger.LogInformation("ResetSession called for session {SessionId}.", sessionId);

        try
        {
            await _chatService.ResetConversationAsync(sessionId);
            return Ok(new { message = $"Session '{sessionId}' has been reset successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ResetSession for session {SessionId}.", sessionId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred while resetting the session." });
        }
    }
}
