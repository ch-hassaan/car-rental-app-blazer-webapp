using RentalBlazorApp.Models.AI;
using RentalBlazorApp.Services.AI.Interfaces;

namespace RentalBlazorApp.Services.AI;


public sealed class ChatService : IChatService
{
    private readonly IGroqService _groqService;
    private readonly IPromptService _promptService;
    private readonly ILogger<ChatService> _logger;

    
    public ChatService(
        IGroqService groqService,
        IPromptService promptService,
        ILogger<ChatService> logger)
    {
        _groqService   = groqService;
        _promptService = promptService;
        _logger        = logger;
    }

    
    public Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        
        
        throw new NotImplementedException("ChatService.ProcessMessageAsync will be implemented in the next phase.");
    }

    
    public Task ResetConversationAsync(string sessionId)
    {
        
        
        throw new NotImplementedException("ChatService.ResetConversationAsync will be implemented in the next phase.");
    }
}
