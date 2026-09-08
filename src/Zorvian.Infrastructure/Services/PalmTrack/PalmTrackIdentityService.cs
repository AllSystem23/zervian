using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Core.Entities;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services.PalmTrack;

/// <summary>
/// Resolves PalmTrack organization IDs to Zorvian tenant IDs via ExternalIdentityMapping.
/// </summary>
public sealed class PalmTrackIdentityService : IPalmTrackIdentityService
{
    private readonly ZorvianDbContext _db;
    private readonly ILogger<PalmTrackIdentityService> _logger;

    public PalmTrackIdentityService(ZorvianDbContext db, ILogger<PalmTrackIdentityService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> IsOrganizationReconciledAsync(string palmTrackOrgId)
    {
        return await _db.Set<ExternalIdentityMapping>()
            .AnyAsync(m =>
                m.PalmTrackOrgId == palmTrackOrgId &&
                m.IsActive);
    }

    public async Task<Guid?> GetTenantIdAsync(string palmTrackOrgId)
    {
        var mapping = await _db.Set<ExternalIdentityMapping>()
            .FirstOrDefaultAsync(m =>
                m.PalmTrackOrgId == palmTrackOrgId &&
                m.IsActive);

        if (mapping == null)
        {
            _logger.LogWarning("No reconciled mapping for PalmTrack org {OrgId}", palmTrackOrgId);
            return null;
        }

        if (Guid.TryParse(mapping.ZorvianTenantId, out var tenantId))
            return tenantId;

        _logger.LogError("Invalid ZorvianTenantId format in mapping for org {OrgId}: {TenantId}",
            palmTrackOrgId, mapping.ZorvianTenantId);
        return null;
    }
}
