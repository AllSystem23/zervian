using System.Text.Json;
using Zorvian.Application.Messages;

namespace Zorvian.Application.Interfaces.PalmTrack;

/// <summary>
/// Maps PalmTrack webhook events to Zorvian entity persistence operations.
/// Plan §8.2: dispatches to specific handlers per event type.
/// </summary>
public interface IPalmTrackEventMapper
{
    /// <summary>
    /// Processes a PalmTrack webhook event, mapping it to Zorvian entities.
    /// </summary>
    Task<MappingResult> ProcessAsync(PalmTrackWebhookReceived message);
}

public sealed class MappingResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static MappingResult Ok(string message) => new() { Success = true, Message = message };
    public static MappingResult Fail(string message) => new() { Success = false, Message = message };
}
