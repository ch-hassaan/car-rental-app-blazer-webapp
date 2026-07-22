using RentalBlazorApp.Models.AI;

namespace RentalBlazorApp.Services.AI.Interfaces;


public interface IChatService
{
    
    
    Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default);

    
    Task ResetConversationAsync(string sessionId);
}
