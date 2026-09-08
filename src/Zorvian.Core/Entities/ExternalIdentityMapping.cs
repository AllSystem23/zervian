namespace Zorvian.Core.Entities;

/// <summary>
/// Maps PalmTrack organization IDs to Zorvian tenant IDs.
/// Used for identity reconciliation during webhook processing.
/// </summary>
public sealed class ExternalIdentityMapping : BaseEntity
{
    /// <summary>PalmTrack organization ID.</summary>
    public string PalmTrackOrgId { get; set; } = string.Empty;

    /// <summary>Zorvian tenant ID (Guid as string).</summary>
    public string ZorvianTenantId { get; set; } = string.Empty;

    /// <summary>PalmTrack organization display name.</summary>
    public string? PalmTrackOrgName { get; set; }

    /// <summary>Zorvian company/tenant display name.</summary>
    public string? ZorvianTenantName { get; set; }

    /// <summary>Whether this mapping is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Last time this mapping was synced/verified.</summary>
    public DateTime LastSyncedAt { get; set; }
}
