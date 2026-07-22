using System.Text.Json.Serialization;

namespace RentalBlazorApp.Models.AI;


public sealed class GroqResponse
{
    
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    
    [JsonPropertyName("choices")]
    public List<GroqChoiceDto> Choices { get; set; } = [];

    
    [JsonPropertyName("usage")]
    public GroqUsageDto? Usage { get; set; }
}


public sealed class GroqChoiceDto
{
    
    
    [JsonPropertyName("index")]
    public int Index { get; set; }

    
    [JsonPropertyName("message")]
    public GroqMessageDto? Message { get; set; }

    
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}


public sealed class GroqUsageDto
{
    
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
