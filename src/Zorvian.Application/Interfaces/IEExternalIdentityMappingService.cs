using Zorvian.Core.Entities;

namespace Zorvian.Application.Interfaces;

/// <summary>
/// Gestión de mapeos de identidad externa (PalmTrack org → Zorvian tenant).
/// Usado para SSO y validación de webhooks entrantes.
/// </summary>
public interface IExternalIdentityMappingService
{
    /// <summary>
    /// Lista todos los mapeos existentes.
    /// </summary>
    Task<List<ExternalIdentityMapping>> GetAllAsync();

    /// <summary>
    /// Crea un nuevo mapeo de organización PalmTrack a tenant Zorvian.
    /// </summary>
    Task<ExternalIdentityMapping> CreateAsync(
        string palmTrackOrgId,
        Guid zorvianTenantId,
        string? palmTrackOrgName = null,
        string? zorvianTenantName = null);

    /// <summary>
    /// Elimina un mapeo por su ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Activa/desactiva un mapeo existente.
    /// </summary>
    Task<bool> SetActiveAsync(Guid id, bool isActive);

    /// <summary>
    /// Busca un mapeo por organización PalmTrack.
    /// </summary>
    Task<ExternalIdentityMapping?> GetByPalmTrackOrgIdAsync(string palmTrackOrgId);
}
