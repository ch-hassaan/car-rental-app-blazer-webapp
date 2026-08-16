namespace RentalBlazorApp.Models.AI;

/// <summary>
/// Represents a single chat message as it appears in the UI.
/// This is a VIEW MODEL — it lives only on the client and is never sent to the backend.
///
/// Role values: "user" | "ai" | "error"
/// </summary>
public sealed class UiChatMessage
{
    /// <summary>Unique identifier for keying Blazor list rendering (@key).</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>"user", "ai", or "error"</summary>
    public string Role { get; init; } = "user";

    /// <summary>Raw text content of the message.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>When the message was added to the conversation.</summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;

    /// <summary>True if this message is an error placeholder with a retry button.</summary>
    public bool IsError { get; init; }

    /// <summary>
    /// The original user text that triggered this error — stored so the retry
    /// button can re-send exactly the same content.
    /// </summary>
    public string? RetryPayload { get; init; }
}
