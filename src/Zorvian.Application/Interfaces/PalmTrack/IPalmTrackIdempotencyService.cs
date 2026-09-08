namespace Zorvian.Application.Interfaces.PalmTrack;

/// <summary>
/// Manages idempotency for PalmTrack webhook events.
/// </summary>
public interface IPalmTrackIdempotencyService
{
    /// <summary>
    /// Checks if an idempotency key has already been processed.
    /// </summary>
    Task<bool> IsProcessedAsync(string idempotencyKey);

    /// <summary>
    /// Marks an idempotency key as processed with event metadata.
    /// </summary>
    Task MarkProcessedAsync(string idempotencyKey, string eventName, string organizationId, string? payload = null);

    /// <summary>
    /// Marks a previously processed key as failed (for retry scenarios).
    /// </summary>
    Task MarkFailedAsync(string idempotencyKey, string error);
}
