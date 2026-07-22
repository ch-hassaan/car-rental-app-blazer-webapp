namespace RentalBlazorApp.Models.AI;


public sealed class ChatResponse
{
    
    
    public string? Message { get; set; }

    
    public bool IsSuccess { get; set; }

    
    public string? ErrorMessage { get; set; }

    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    
    public int? TotalTokensUsed { get; set; }
}
