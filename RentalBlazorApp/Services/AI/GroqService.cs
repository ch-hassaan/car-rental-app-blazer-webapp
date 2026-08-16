using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RentalBlazorApp.Configuration;
using RentalBlazorApp.Models.AI;
using RentalBlazorApp.Services.AI.Interfaces;

namespace RentalBlazorApp.Services.AI;

/// <summary>
/// Sends requests to the Groq Chat Completions API over HTTP.
///
/// The HttpClient is pre-configured in Program.cs via AddHttpClient:
///   - BaseAddress = GroqSettings.BaseUrl  (e.g. "https://api.groq.com/openai/v1/")
///   - Authorization header = "Bearer {ApiKey}"
///   - Timeout = 30 seconds
///
/// This class is responsible ONLY for the HTTP transport layer.
/// It does NOT build prompts or manage conversation state.
/// </summary>
public class GroqService : IGroqService
{
    // ─────────────────────────────────────────────────────────────────────────
    // Dependencies
    // ─────────────────────────────────────────────────────────────────────────

    private readonly HttpClient _httpClient;
    private readonly GroqSettings _settings;
    private readonly ILogger<GroqService> _logger;

    // Relative endpoint within the BaseUrl.
    // NOTE: Must NOT start with "/" when BaseUrl already ends with "/".
    //       A leading "/" would strip the base path and break the URI.
    private const string ChatCompletionsEndpoint = "chat/completions";

    // JSON options reused across calls to avoid re-allocating on every request.
    private static readonly JsonSerializerOptions _jsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────

    public GroqService(
        HttpClient httpClient,
        IOptions<GroqSettings> options,
        ILogger<GroqService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings   = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger     = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SendAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Serialises the <see cref="GroqRequest"/> to JSON, POSTs it to the Groq
    /// Chat Completions endpoint, deserialises the response, and returns it.
    ///
    /// Returns <c>null</c> if the API call fails so that callers can handle
    /// the failure gracefully without catching exceptions themselves.
    /// </summary>
    public async Task<GroqResponse?> SendAsync(GroqRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            _logger.LogWarning("SendAsync called with a null GroqRequest.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogError("Groq API key is not configured. Check appsettings.json under Groq:ApiKey.");
            return null;
        }

        try
        {
            // Serialise request to JSON.
            var json = JsonSerializer.Serialize(request);

            _logger.LogDebug(
                "Sending request to Groq. Model: {Model}, Messages: {Count}.",
                request.Model,
                request.Messages.Count);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // POST to "chat/completions" (relative path, resolved against BaseAddress).
            var httpResponse = await _httpClient.PostAsync(
                ChatCompletionsEndpoint,
                content,
                cancellationToken);

            // Non-success → read body for diagnostic info, then return null.
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogError(
                    "Groq API returned HTTP {StatusCode}. Response body: {Body}",
                    (int)httpResponse.StatusCode,
                    errorBody);

                return null;
            }

            // Deserialise the successful response.
            var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            var groqResponse = JsonSerializer.Deserialize<GroqResponse>(responseJson, _jsonReadOptions);

            if (groqResponse is null)
            {
                _logger.LogError("Failed to deserialise Groq response. Raw JSON: {Json}", responseJson);
                return null;
            }

            _logger.LogDebug(
                "Groq responded successfully. Choices: {Count}, TotalTokens: {Tokens}.",
                groqResponse.Choices.Count,
                groqResponse.Usage?.TotalTokens);

            return groqResponse;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !cancellationToken.IsCancellationRequested)
        {
            // The HttpClient.Timeout of 30 s was hit.
            _logger.LogError(ex, "Groq API request timed out after {Timeout} seconds.", _httpClient.Timeout.TotalSeconds);
            return null;
        }
        catch (OperationCanceledException)
        {
            // The caller's CancellationToken fired (e.g. browser navigated away).
            _logger.LogWarning("Groq API request was cancelled by the caller.");
            return null;
        }
        catch (HttpRequestException ex)
        {
            // Network-level failure (DNS, connection refused, etc.).
            _logger.LogError(ex, "HTTP network error while contacting Groq API.");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Groq API response as JSON.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GroqService.SendAsync.");
            return null;
        }
    }
}