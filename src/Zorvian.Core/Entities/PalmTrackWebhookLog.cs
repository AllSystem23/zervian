namespace Zorvian.Core.Entities;

/// <summary>
/// Stores webhook delivery logs from PalmTrack for audit and idempotency.
/// </summary>
public sealed class PalmTrackWebhookLog : BaseEntity
{
    /// <summary>Idempotency key from X-Idempotency-Key header.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>Event name from X-Webhook-Event header.</summary>
    public string Event { get; set; } = string.Empty;

    /// <summary>Organization ID from PalmTrack (from payload organizationId field).</summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>Zorvian tenant ID after reconciliation (nullable if org not reconciled).</summary>
    public Guid? ZorvianTenantId { get; set; }

    /// <summary>Full JSON payload.</summary>
    public string? Payload { get; set; }

    /// <summary>Status: received, processed, failed, dlq.</summary>
    public string Status { get; set; } = "received";

    /// <summary>HTTP status code sent back to PalmTrack.</summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>Error message if failed.</summary>
    public string? Error { get; set; }

    /// <summary>When the webhook was received.</summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When processing completed (nullable).</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Processing duration in milliseconds.</summary>
    public int? DurationMs { get; set; }
}
