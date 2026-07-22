namespace RentalBlazorApp.Models.AI;


public sealed class ConversationContext
{
    
    
    public string SessionId { get; set; } = string.Empty;

    
    public string? UserId { get; set; }

    
    public List<ChatMessage> Messages { get; set; } = [];

    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    
    public Dictionary<string, string> Metadata { get; set; } = [];
}
