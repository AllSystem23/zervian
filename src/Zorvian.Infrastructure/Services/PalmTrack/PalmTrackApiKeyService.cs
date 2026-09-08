using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Core.Entities;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services.PalmTrack;

/// <summary>
/// Validates API keys for PalmTrack read endpoints (plan §1.1).
/// Uses the existing ApiKey entity with SHA256 hash comparison.
/// </summary>
public sealed class PalmTrackApiKeyService : IPalmTrackApiKeyService
{
    private readonly ZorvianDbContext _db;
    private readonly ILogger<PalmTrackApiKeyService> _logger;

    public PalmTrackApiKeyService(ZorvianDbContext db, ILogger<PalmTrackApiKeyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey, string organizationId)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(organizationId))
            return false;

        var keyHash = ComputeHash(apiKey);

        var key = await _db.Set<ApiKey>()
            .FirstOrDefaultAsync(k =>
                k.KeyHash == keyHash &&
                k.IsActive &&
                !k.IsDeleted);

        if (key == null)
        {
            _logger.LogWarning("Invalid API key attempt for org {OrgId}", organizationId);
            return false;
        }

        // Check expiration
        if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired API key used for org {OrgId}", organizationId);
            return false;
        }

        // Update last used timestamp
        key.LastUsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<string?> GetOrganizationIdAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        var keyHash = ComputeHash(apiKey);

        var key = await _db.Set<ApiKey>()
            .FirstOrDefaultAsync(k =>
                k.KeyHash == keyHash &&
                k.IsActive &&
                !k.IsDeleted);

        // ApiKey doesn't have TenantId directly; return the key's Name as identifier
        // In production, map via a separate table or convention
        return key?.Name;
    }

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
