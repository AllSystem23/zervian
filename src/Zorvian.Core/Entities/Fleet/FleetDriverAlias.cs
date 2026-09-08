namespace Zorvian.Core.Entities.Fleet;

/// <summary>
/// Maps driver aliases from external systems (PalmTrack) to Zorvian Driver entities.
/// Supports multiple aliases per driver for flexible matching during webhook processing.
/// </summary>
public sealed class FleetDriverAlias : BaseEntity
{
    /// <summary>Zorvian Driver ID.</summary>
    public Guid DriverId { get; set; }

    /// <summary>Driver navigation property.</summary>
    public Driver Driver { get; set; } = null!;

    /// <summary>External system name (e.g., "palmtrack").</summary>
    public string ExternalSystem { get; set; } = string.Empty;

    /// <summary>Driver name as reported by the external system.</summary>
    public string ExternalName { get; set; } = string.Empty;

    /// <summary>Driver ID as reported by the external system.</summary>
    public string? ExternalDriverId { get; set; }

    /// <summary>Match confidence: "exact", "fuzzy", "manual".</summary>
    public string MatchType { get; set; } = "manual";

    /// <summary>Whether this alias is the primary mapping.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Number of successful matches using this alias.</summary>
    public int MatchCount { get; set; }
}
