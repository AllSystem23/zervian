using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zorvian.Application.Interfaces;
using Zorvian.Web.Authorization;

namespace Zorvian.Web.Controllers.Admin;

[ApiController]
[Authorize]
[Route("admin/palmtrack/identity-mappings")]
public sealed class ExternalIdentityMappingsController : ControllerBase
{
    private readonly IExternalIdentityMappingService _service;

    public ExternalIdentityMappingsController(
        IExternalIdentityMappingService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /admin/palmtrack/identity-mappings
    /// Lista todos los mapeos de identidad externa existentes.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.SettingsRead)]
    public async Task<IActionResult> GetAll()
    {
        var mappings = await _service.GetAllAsync();
        return Ok(new { mappings });
    }

    /// <summary>
    /// POST /admin/palmtrack/identity-mappings
    /// Crea un nuevo mapeo de organización PalmTrack a tenant Zorvian.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.SettingsWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExternalIdentityMappingRequest request)
    {
        try
        {
            var mapping = await _service.CreateAsync(
                request.PalmTrackOrgId,
                request.ZorvianTenantId,
                request.PalmTrackOrgName,
                request.ZorvianTenantName);

            return Ok(new { mapping });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /admin/palmtrack/identity-mappings/{id}
    /// Elimina (logico) un mapeo de identidad externa.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.SettingsWrite)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = "Mapping not found" });

        return Ok(new { message = "Mapping deleted successfully" });
    }

    /// <summary>
    /// PATCH /admin/palmtrack/identity-mappings/{id}/active
    /// Activa o desactiva un mapeo existente.
    /// </summary>
    [HttpPatch("{id:guid}/active")]
    [RequirePermission(Permissions.SettingsWrite)]
    public async Task<IActionResult> SetActive(
        Guid id,
        [FromQuery] bool active)
    {
        var result = await _service.SetActiveAsync(id, active);
        if (!result)
            return NotFound(new { error = "Mapping not found" });

        return Ok(new { message = active ? "Mapping activated" : "Mapping deactivated" });
    }

    /// <summary>
    /// GET /admin/palmtrack/identity-mappings/sync-status
    /// Retorna el estado actual de la reconciliación de identidades:
    /// cantidad de mapeos activos, últimos sync, etc.
    /// </summary>
    [HttpGet("sync-status")]
    [RequirePermission(Permissions.SettingsRead)]
    public async Task<IActionResult> GetSyncStatus()
    {
        var allMappings = await _service.GetAllAsync();
        var activeMappings = allMappings.Count(m => m.IsActive);
        var inactiveMappings = allMappings.Count(m => !m.IsActive);

        return Ok(new
        {
            totalMappings = allMappings.Count,
            activeMappings,
            inactiveMappings,
            lastUpdated = DateTime.UtcNow,
        });
    }
}

/// <summary>
/// Request body para crear un mapeo de identidad externa.
/// </summary>
public sealed record CreateExternalIdentityMappingRequest(
    string PalmTrackOrgId,
    Guid ZorvianTenantId,
    string? PalmTrackOrgName = null,
    string? ZorvianTenantName = null
);
