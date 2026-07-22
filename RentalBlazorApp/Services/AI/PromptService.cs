using Microsoft.Extensions.Options;
using RentalBlazorApp.Configuration;
using RentalBlazorApp.Models.AI;
using RentalBlazorApp.Services.AI.Interfaces;

namespace RentalBlazorApp.Services.AI;

public sealed class PromptService : IPromptService
{
    private readonly GroqSettings _settings;
    private readonly ILogger<PromptService> _logger;

    public PromptService(
        IOptions<GroqSettings> settings,
        ILogger<PromptService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<GroqRequest> BuildPromptAsync(
        ConversationContext context,
        string userMessage)
    {
        var request = new GroqRequest
        {
            Model = _settings.ModelName,
            Messages = new List<GroqMessageDto>()
        };

        // Add the system prompt
        request.Messages.Add(new GroqMessageDto
        {
            Role = "system",
            Content = BuildSystemPrompt()
        });

        // Add previous conversation history
        if (context.Messages is not null && context.Messages.Count > 0)
        {
            foreach (var message in context.Messages)
            {
                request.Messages.Add(new GroqMessageDto
                {
                    Role = message.Role,
                    Content = message.Content
                });
            }
        }

        // Add the current user message
        request.Messages.Add(new GroqMessageDto
        {
            Role = "user",
            Content = userMessage
        });

        _logger.LogInformation(
            "Prompt built successfully for session {SessionId}. Total messages: {MessageCount}",
            context.SessionId,
            request.Messages.Count);

        return Task.FromResult(request);
    }

    public string BuildSystemPrompt()
    {
        return """
You are the AI Rental Assistant for PDM Rentals.

Your responsibilities are:

- Help customers choose the most suitable rental vehicle.
- Explain rental policies and booking procedures.
- Recommend vehicles based on customer requirements.
- Answer questions related to cars, bookings, pricing, and rentals.
- Be professional, friendly, and concise.
- If a user asks something unrelated to PDM Rentals or vehicle rentals, politely explain that your assistance is limited to rental-related topics.

Always provide clear and accurate responses.
""";
    }
}