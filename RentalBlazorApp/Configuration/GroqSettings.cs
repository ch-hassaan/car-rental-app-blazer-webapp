namespace RentalBlazorApp.Configuration;

public class GroqSettings
{
    public const string SectionName = "Groq";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;
}