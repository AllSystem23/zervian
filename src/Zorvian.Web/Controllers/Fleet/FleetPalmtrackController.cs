using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zorvian.Core.Entities.Fleet;
using Zorvian.Infrastructure.Data;
using Zorvian.Infrastructure.Services.Fleet;
using Zorvian.Web.Authorization;
using Zorvian.Core.Interfaces;

namespace Zorvian.Web.Controllers.Fleet;

[ApiController]
[Authorize]
[Route("zorvian/v1/fleet/palmtrack")]
public sealed class FleetPalmtrackController : ControllerBase
{
    private readonly PalmTrackFleetMapper _mapper;
    private readonly ZorvianDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ILogger<FleetPalmtrackController> _logger;

    public FleetPalmtrackController(
        PalmTrackFleetMapper mapper,
        ZorvianDbContext db,
        ITenantContext tenant,
        ILogger<FleetPalmtrackController> logger)
    {
        _mapper = mapper;
        _db = db;
        _tenant = tenant;
        _logger = logger;
    }

    /// <summary>
    /// GET /fleet/palmtrack/stats
    /// Retorna estadísticas de sincronización PalmTrack usando PalmTrackFleetMapper.
    /// </summary>
    [HttpGet("stats")]
    [RequirePermission(Permissions.FleetRead)]
    public async Task<IActionResult> GetStats()
    {
        var tenantId = _tenant.TenantId.Value;
        if (tenantId == Guid.Empty) return Unauthorized();

        var tenantIdStr = tenantId.ToString();

        var stats = await _mapper.GetSyncStatsAsync();

        // Obtener últimos errores (últimos 10) con filtro de tenant
        var lastErrors = await _db.Set<FleetExternalReference>()
            .Where(r => r.TenantId == tenantIdStr
                && r.ExternalSystem == "palmtrack"
                && r.Status == "error"
                && r.LastError != null)
            .OrderByDescending(r => r.UpdatedAt)
            .Take(10)
            .Select(r => new
            {
                r.Id,
                r.EntityType,
                r.EntityId,
                r.ExternalId,
                r.LastError,
                r.UpdatedAt,
            })
            .ToListAsync();

        return Ok(new
        {
            totalReferences = stats.TotalReferences,
            syncedCount = stats.SyncedCount,
            pendingCount = stats.PendingCount,
            conflictCount = stats.ConflictCount,
            errorCount = stats.ErrorCount,
            syncRate = stats.TotalReferences > 0
                ? (double)stats.SyncedCount / stats.TotalReferences
                : 0,
            vehicleCount = stats.VehicleCount,
            driverAliasCount = stats.DriverAliasCount,
            lastErrors,
            lastSyncAt = stats.LastSyncAt ?? DateTime.UtcNow,
        });
    }

    /// <summary>
    /// GET /fleet/palmtrack/references
    /// Lista las referencias externas sincronizadas usando PalmTrackFleetMapper.
    /// </summary>
    [HttpGet("references")]
    [RequirePermission(Permissions.FleetRead)]
    public async Task<IActionResult> GetReferences(
        [FromQuery] string? entityType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var tenantId = _tenant.TenantId.Value;
        if (tenantId == Guid.Empty) return Unauthorized();

        var tenantIdStr = tenantId.ToString();

        var allRefs = await _mapper.GetAllReferencesAsync(entityType);

        // Apply tenant filter
        allRefs = allRefs.Where(r => r.TenantId == tenantIdStr).ToList();

        // Apply entity type filter if provided
        if (!string.IsNullOrWhiteSpace(entityType))
            allRefs = allRefs.Where(r => r.EntityType == entityType).ToList();

        var total = allRefs.Count;
        var items = allRefs
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.EntityType,
                r.EntityId,
                r.ExternalId,
                r.ExternalPayload,
                r.LastSyncAt,
                r.SyncDirection,
                r.Status,
                r.LastError,
                r.ConsecutiveFailures,
                r.CreatedAt,
                r.UpdatedAt,
            })
            .ToList();

        return Ok(new
        {
            items,
            page,
            pageSize,
            total,
            hasMore = page * pageSize < total,
        });
    }

    /// <summary>
    /// GET /fleet/palmtrack/conflicts
    /// Lista las referencias en estado conflict usando PalmTrackFleetMapper.
    /// </summary>
    [HttpGet("conflicts")]
    [RequirePermission(Permissions.FleetRead)]
    public async Task<IActionResult> GetConflicts()
    {
        var tenantId = _tenant.TenantId.Value;
        if (tenantId == Guid.Empty) return Unauthorized();

        var tenantIdStr = tenantId.ToString();

        var allConflicts = await _mapper.GetConflictsAsync();
        var conflicts = allConflicts
            .Where(r => r.TenantId == tenantIdStr)
            .Select(r => new
            {
                r.Id,
                r.EntityType,
                r.EntityId,
                r.ExternalId,
                r.ExternalPayload,
                r.LastError,
                r.ConsecutiveFailures,
                r.CreatedAt,
                r.UpdatedAt,
            }).ToList();

        return Ok(new { conflicts });
    }

    /// <summary>
    /// POST /fleet/palmtrack/conflicts/{referenceId}/resolve
    /// Resuelve un conflicto de sincronización usando PalmTrackFleetMapper.
    /// </summary>
    [HttpPost("conflicts/{referenceId:guid}/resolve")]
    [RequirePermission(Permissions.FleetWrite)]
    public async Task<IActionResult> ResolveConflict(
        Guid referenceId,
        [FromBody] ResolveConflictRequest request)
    {
        var tenantId = _tenant.TenantId.Value;
        if (tenantId == Guid.Empty) return Unauthorized();

        var tenantIdStr = tenantId.ToString();

        var reference = await _db.Set<FleetExternalReference>()
            .FirstOrDefaultAsync(r => r.Id == referenceId && r.TenantId == tenantIdStr);

        if (reference == null)
            return NotFound(new { error = "Reference not found" });

        if (reference.Status != "conflict")
            return BadRequest(new { error = "Reference is not in conflict status" });

        await _mapper.ResolveConflictAsync(referenceId, request.AcceptExternal);

        return Ok(new { status = "resolved", referenceId, acceptExternal = request.AcceptExternal });
    }


}

/// <summary>
/// Request body para resolución de conflictos.
/// </summary>
public sealed record ResolveConflictRequest(
    bool AcceptExternal
);
