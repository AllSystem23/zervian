using System.Text.Json;

namespace Zorvian.Application.Messages;

/// <summary>
/// MassTransit message published when a PalmTrack webhook is received and validated.
/// Consumed by: PalmTrackWebhookConsumer.
/// </summary>
public sealed record PalmTrackWebhookReceived
{
    public string Event { get; init; } = string.Empty;
    public string OrganizationId { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public JsonElement Payload { get; init; }
    public DateTime ReceivedAt { get; init; }
}

/// <summary>
/// Published when a PalmTrack webhook is processed successfully.
/// </summary>
public sealed record PalmTrackWebhookProcessed
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public string Status { get; init; } = "processed";
    public DateTime ProcessedAt { get; init; }
}

/// <summary>
/// Published when a PalmTrack webhook fails processing.
/// </summary>
public sealed record PalmTrackWebhookFailed
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public string Event { get; init; } = string.Empty;
    public string OrganizationId { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
    public JsonElement Payload { get; init; }
}
