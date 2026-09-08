namespace Zorvian.Application.Interfaces.PalmTrack;

/// <summary>
/// Resolves PalmTrack organization IDs to Zorvian tenant IDs.
/// </summary>
public interface IPalmTrackIdentityService
{
    /// <summary>
    /// Checks if a PalmTrack organization has been reconciled to a Zorvian tenant.
    /// </summary>
    Task<bool> IsOrganizationReconciledAsync(string palmTrackOrgId);

    /// <summary>
    /// Gets the Zorvian tenant ID for a PalmTrack organization.
    /// </summary>
    Task<Guid?> GetTenantIdAsync(string palmTrackOrgId);
}
