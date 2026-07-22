using RentalBlazorApp.Models.AI;

namespace RentalBlazorApp.Services.AI.Interfaces;


public interface IPromptService
{
    
    
    Task<GroqRequest> BuildPromptAsync(ConversationContext context, string userMessage);

    
    string BuildSystemPrompt();
}
