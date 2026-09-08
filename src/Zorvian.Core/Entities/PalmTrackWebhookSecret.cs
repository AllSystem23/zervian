namespace Zorvian.Core.Entities;

/// <summary>
/// Stores HMAC-SHA256 secrets for validating PalmTrack webhook signatures.
/// Supports secret rotation with valid-from/to dates.
/// </summary>
public sealed class PalmTrackWebhookSecret : BaseEntity
{
    /// <summary>PalmTrack organization ID this secret belongs to.</summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>Hashed secret (plaintext only exists in memory during validation).</summary>
    public string SecretHash { get; set; } = string.Empty;

    /// <summary>First 8 chars of secret for identification in logs (NOT the secret itself).</summary>
    public string SecretPrefix { get; set; } = string.Empty;

    /// <summary>When this secret becomes valid.</summary>
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    /// <summary>When this secret expires (null = never).</summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>Whether this secret is currently active.</summary>
    public bool IsActive { get; set; } = true;
}
