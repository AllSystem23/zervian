namespace Zorvian.Application.Messages;

/// <summary>
/// Plan Paso 6 §5.1 — MassTransit event for user changes from PalmTrack.
/// 
/// Published when a PalmTrack webhook delivers a user.updated event.
/// Consumed by: PalmTrackWebhookConsumer (or dedicated UserUpdatedConsumer).
/// 
/// Supported actions:
/// - role_changed: Update UserRoles in Zorvian
/// - deactivated: Set User.IsActive = false
/// - reactivated: Set User.IsActive = true
/// - org_changed: Update TenantId (requires reconciliation)
/// </summary>
public sealed record UserUpdatedEvent
{
    /// <summary>PalmTrack Firebase UID of the user.</summary>
    public string FirebaseUid { get; init; } = string.Empty;

    /// <summary>The type of change: role_changed, deactivated, reactivated, org_changed.</summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>PalmTrack role after change (for role_changed action).</summary>
    public string? NewRole { get; init; }

    /// <summary>PalmTrack organization ID after change (for org_changed action).</summary>
    public string? NewOrgId { get; init; }

    /// <summary>Timestamp of the change in PalmTrack.</summary>
    public DateTime OccurredAt { get; init; }

    /// <summary>Source of the event (for loop prevention).</summary>
    public string Source { get; init; } = "palmtrack";

    /// <summary>Idempotency key to prevent duplicate processing.</summary>
    public string IdempotencyKey { get; init; } = string.Empty;
}
