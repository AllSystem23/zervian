namespace Zorvian.Core.Entities;

/// <summary>
/// Dead Letter Queue for PalmTrack webhook events that failed after max retries.
/// </summary>
public sealed class PalmTrackWebhookDlq : BaseEntity
{
    /// <summary>Idempotency key from the original event.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>Event name.</summary>
    public string Event { get; set; } = string.Empty;

    /// <summary>PalmTrack organization ID.</summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>Full JSON payload.</summary>
    public string? Payload { get; set; }

    /// <summary>Error message that caused the failure.</summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>When the event moved to DLQ.</summary>
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Number of retries attempted before DLQ.</summary>
    public int RetryCount { get; set; }

    /// <summary>Whether the event has been manually resolved.</summary>
    public bool IsResolved { get; set; }

    /// <summary>When it was resolved (if applicable).</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>User who resolved it.</summary>
    public string? ResolvedBy { get; set; }
}
