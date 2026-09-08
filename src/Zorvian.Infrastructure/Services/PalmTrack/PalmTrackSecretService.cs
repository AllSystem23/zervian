using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Core.Entities;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services.PalmTrack;

/// <summary>
/// Manages HMAC-SHA256 secrets for PalmTrack webhook validation.
/// Supports secret rotation with valid-from/to dates.
/// </summary>
public sealed class PalmTrackSecretService : IPalmTrackSecretService
{
    private readonly ZorvianDbContext _db;
    private readonly ILogger<PalmTrackSecretService> _logger;

    public PalmTrackSecretService(ZorvianDbContext db, ILogger<PalmTrackSecretService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<string?> GetActiveSecretAsync(string organizationId)
    {
        var now = DateTime.UtcNow;
        var secret = await _db.Set<PalmTrackWebhookSecret>()
            .Where(s =>
                s.OrganizationId == organizationId &&
                s.IsActive &&
                s.ValidFrom <= now &&
                (s.ValidTo == null || s.ValidTo > now))
            .OrderByDescending(s => s.ValidFrom)
            .FirstOrDefaultAsync();

        return secret?.SecretHash; // SecretHash stores the plaintext secret for validation
    }

    public async Task<string?> FindMatchingSecretAsync(string organizationId, string signature, string body)
    {
        var now = DateTime.UtcNow;
        var secrets = await _db.Set<PalmTrackWebhookSecret>()
            .Where(s =>
                s.OrganizationId == organizationId &&
                s.IsActive &&
                s.ValidFrom <= now &&
                (s.ValidTo == null || s.ValidTo > now))
            .ToListAsync();

        foreach (var secret in secrets)
        {
            if (VerifyHmac(body, signature, secret.SecretHash))
            {
                return secret.SecretHash;
            }
        }

        _logger.LogWarning("No matching HMAC secret found for org {OrgId}", organizationId);
        return null;
    }

    private static bool VerifyHmac(string body, string providedSignature, string secret)
    {
        if (string.IsNullOrEmpty(providedSignature) || string.IsNullOrEmpty(secret))
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(providedSignature));
    }
}
