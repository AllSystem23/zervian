namespace Zorvian.Application.Interfaces.PalmTrack;

/// <summary>
/// Manages HMAC-SHA256 secrets for PalmTrack webhook validation.
/// </summary>
public interface IPalmTrackSecretService
{
    /// <summary>
    /// Gets the active plaintext secret for the given organization.
    /// Returns null if no active secret exists.
    /// </summary>
    Task<string?> GetActiveSecretAsync(string organizationId);

    /// <summary>
    /// Tries all active secrets for the organization against the signature.
    /// Returns the matching secret or null.
    /// </summary>
    Task<string?> FindMatchingSecretAsync(string organizationId, string signature, string body);
}
