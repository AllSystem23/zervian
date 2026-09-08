namespace Zorvian.Core.Entities.Fleet;

/// <summary>
/// Maps external system entities (e.g., PalmTrack) to Zorvian Fleet entities.
/// Supports bidirectional sync with conflict tracking.
/// </summary>
public sealed class FleetExternalReference : BaseEntity
{
    /// <summary>External system identifier (e.g., "palmtrack").</summary>
    public string ExternalSystem { get; set; } = string.Empty;

    /// <summary>Entity type in Zorvian (e.g., "Vehicle", "Driver", "Trip").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Zorvian entity ID.</summary>
    public Guid EntityId { get; set; }

    /// <summary>External system entity ID.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>JSON snapshot of the external entity data at last sync.</summary>
    public string? ExternalPayload { get; set; }

    /// <summary>Last successful sync timestamp.</summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>Sync direction: "inbound", "outbound", "bidirectional".</summary>
    public string SyncDirection { get; set; } = "bidirectional";

    /// <summary>Current sync status: "synced", "pending", "conflict", "error".</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Error message if last sync failed.</summary>
    public string? LastError { get; set; }

    /// <summary>Number of consecutive sync failures.</summary>
    public int ConsecutiveFailures { get; set; }
}
