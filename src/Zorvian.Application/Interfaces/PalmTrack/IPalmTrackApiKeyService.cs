namespace Zorvian.Application.Interfaces.PalmTrack;

/// <summary>
/// Validates API keys for PalmTrack read endpoints (plan §1.1).
/// Reusable across all /api/palm/v1/* read controllers.
/// </summary>
public interface IPalmTrackApiKeyService
{
    /// <summary>
    /// Validates that the provided API key is active and belongs to the given organization.
    /// </summary>
    Task<bool> ValidateApiKeyAsync(string apiKey, string organizationId);

    /// <summary>
    /// Gets the organization ID associated with an API key.
    /// Returns null if the key is invalid or inactive.
    /// </summary>
    Task<string?> GetOrganizationIdAsync(string apiKey);
}
