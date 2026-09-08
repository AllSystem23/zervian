using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces;
using Zorvian.Core.Entities;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de gestión de mapeos de identidad externa.
/// Permite crear, listar, activar/desactivar y eliminar mapeos
/// de organizaciones PalmTrack a tenants Zorvian.
/// </summary>
public sealed class ExternalIdentityMappingService : IExternalIdentityMappingService
{
    private readonly ZorvianDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ExternalIdentityMappingService> _logger;

    public ExternalIdentityMappingService(
        ZorvianDbContext db,
        ITenantContext tenantContext,
        ILogger<ExternalIdentityMappingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ExternalIdentityMapping>> GetAllAsync()
    {
        return await _db.Set<ExternalIdentityMapping>()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.PalmTrackOrgName ?? m.PalmTrackOrgId)
            .ToListAsync();
    }

    public async Task<ExternalIdentityMapping> CreateAsync(
        string palmTrackOrgId,
        Guid zorvianTenantId,
        string? palmTrackOrgName = null,
        string? zorvianTenantName = null)
    {
        // Validar que no exista un mapping activo para esta org
        var existing = await _db.Set<ExternalIdentityMapping>()
            .FirstOrDefaultAsync(m =>
                m.PalmTrackOrgId == palmTrackOrgId
                && m.IsActive
                && !m.IsDeleted);

        if (existing != null)
            throw new InvalidOperationException(
                $"Ya existe un mapeo activo para la organización PalmTrack '{palmTrackOrgId}'");

        // Validar que el tenant existe
        var tenantExists = await _db.Companies
            .AnyAsync(c => c.TenantId == zorvianTenantId.ToString());

        if (!tenantExists)
            throw new InvalidOperationException(
                $"El tenant ID '{zorvianTenantId}' no existe en la base de datos");

        // Validar que el usuario actual tiene acceso a este tenant
        var currentTenantId = _tenantContext.TenantId.Value;
        var isSuperAdmin = _tenantContext.IsSuperAdmin;
        var hasAccessToTenant = isSuperAdmin
            || currentTenantId == zorvianTenantId;

        if (!hasAccessToTenant)
            throw new InvalidOperationException(
                $"No tiene permisos para crear mappings para el tenant '{zorvianTenantId}'");

        var mapping = new ExternalIdentityMapping
        {
            Id = Guid.NewGuid(),
            PalmTrackOrgId = palmTrackOrgId,
            ZorvianTenantId = zorvianTenantId.ToString(),
            PalmTrackOrgName = palmTrackOrgName,
            ZorvianTenantName = zorvianTenantName,
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            TenantId = zorvianTenantId.ToString(),
            CompanyId = zorvianTenantId,
        };

        await _db.Set<ExternalIdentityMapping>().AddAsync(mapping);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "ExternalIdentityMapping created: PalmTrackOrgId={OrgId} → TenantId={TenantId}",
            palmTrackOrgId, zorvianTenantId);

        return mapping;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var mapping = await _db.Set<ExternalIdentityMapping>()
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (mapping == null)
            return false;

        mapping.IsDeleted = true;
        mapping.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "ExternalIdentityMapping deleted: Id={Id}, PalmTrackOrgId={OrgId}",
            id, mapping.PalmTrackOrgId);

        return true;
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive)
    {
        var mapping = await _db.Set<ExternalIdentityMapping>()
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (mapping == null)
            return false;

        mapping.IsActive = isActive;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "ExternalIdentityMapping {Action}: Id={Id}, PalmTrackOrgId={OrgId}",
            isActive ? "activated" : "deactivated", id, mapping.PalmTrackOrgId);

        return true;
    }

    public async Task<ExternalIdentityMapping?> GetByPalmTrackOrgIdAsync(string palmTrackOrgId)
    {
        return await _db.Set<ExternalIdentityMapping>()
            .FirstOrDefaultAsync(m =>
                m.PalmTrackOrgId == palmTrackOrgId
                && m.IsActive
                && !m.IsDeleted);
    }
}
