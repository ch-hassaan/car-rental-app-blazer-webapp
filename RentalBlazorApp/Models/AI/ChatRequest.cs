namespace RentalBlazorApp.Models.AI;


public sealed class ChatRequest
{
    
    
    public string SessionId { get; set; } = string.Empty;

    
    public string Message { get; set; } = string.Empty;

    
    public string? UserId { get; set; }
}
