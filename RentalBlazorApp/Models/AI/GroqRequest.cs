using System.Text.Json.Serialization;

namespace RentalBlazorApp.Models.AI;


public sealed class GroqRequest
{
    
    
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    
    [JsonPropertyName("messages")]
    public List<GroqMessageDto> Messages { get; set; } = [];

    
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 1024;

    
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;
}


public sealed class GroqMessageDto
{
    
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    
}

