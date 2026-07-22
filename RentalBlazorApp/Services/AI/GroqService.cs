using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RentalBlazorApp.Configuration;
using RentalBlazorApp.Models.AI;
using RentalBlazorApp.Services.AI.Interfaces;

namespace RentalBlazorApp.Services.AI;


public class GroqService : IGroqService
{
    private readonly HttpClient _httpClient;
    private readonly GroqSettings _settings;

    public GroqService(
        HttpClient httpClient,
        IOptions<GroqSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<GroqResponse?> SendAsync(GroqRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "/chat/completions",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);

                throw new Exception(
                    $"Groq API Error ({response.StatusCode})\n{error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            var groqResponse = JsonSerializer.Deserialize<GroqResponse>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return groqResponse;
        }
        catch
        {
            return null;
        }
    }
}