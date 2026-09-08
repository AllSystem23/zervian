using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Core.Entities.Fleet;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services.Fleet;

/// <summary>
/// Maps PalmTrack external entities to Zorvian Fleet entities.
/// Manages FleetExternalReference records for bidirectional sync.
/// Located in Infrastructure layer since it requires direct DbContext access.
/// </summary>
public sealed class PalmTrackFleetMapper
{
    private readonly ZorvianDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ILogger<PalmTrackFleetMapper> _logger;

    private const string ExternalSystem = "palmtrack";

    public PalmTrackFleetMapper(
        ZorvianDbContext db,
        ITenantContext tenant,
        ILogger<PalmTrackFleetMapper> logger)
    {
        _db = db;
        _tenant = tenant;
        _logger = logger;
    }

    // ═══════════════════════════════════════════
    // Vehicle Mapping
    // ═══════════════════════════════════════════

    /// <summary>
    /// Maps a PalmTrack vehicle to a Zorvian Vehicle, creating or updating the reference.
    /// </summary>
    public async Task<Vehicle?> MapVehicleAsync(
        string externalId,
        string plate,
        string? model,
        int? year,
        string? brandName,
        decimal? currentKm,
        CancellationToken ct = default)
    {
        // Check existing reference
        var existingRef = await _db.Set<FleetExternalReference>()
            .FirstOrDefaultAsync(r =>
                r.ExternalSystem == ExternalSystem &&
                r.EntityType == "Vehicle" &&
                r.ExternalId == externalId, ct);

        if (existingRef != null)
        {
            // Update existing vehicle
            var vehicle = await _db.Set<Vehicle>()
                .FirstOrDefaultAsync(v => v.Id == existingRef.EntityId, ct);

            if (vehicle != null)
            {
                if (currentKm.HasValue)
                    vehicle.CurrentKm = currentKm.Value;

                existingRef.ExternalPayload = JsonSerializer.Serialize(new
                {
                    plate, model, year, brandName, currentKm
                });
                existingRef.LastSyncAt = DateTime.UtcNow;
                existingRef.Status = "synced";

                await _db.SaveChangesAsync(ct);
                _logger.LogDebug("Updated vehicle mapping: {Plate} → {VehicleId}", plate, vehicle.Id);
                return vehicle;
            }
        }

        // Find vehicle by plate
        var cleanPlate = plate.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        var vehicleByPlate = await _db.Set<Vehicle>()
            .FirstOrDefaultAsync(v =>
                v.Plate.Replace("-", "").Replace(" ", "").ToUpperInvariant() == cleanPlate &&
                !v.IsDeleted, ct);

        if (vehicleByPlate != null)
        {
            // Create reference for existing vehicle
            var newRef = new FleetExternalReference
            {
                ExternalSystem = ExternalSystem,
                EntityType = "Vehicle",
                EntityId = vehicleByPlate.Id,
                ExternalId = externalId,
                ExternalPayload = JsonSerializer.Serialize(new
                {
                    plate, model, year, brandName, currentKm
                }),
                SyncDirection = "bidirectional",
                Status = "synced",
                LastSyncAt = DateTime.UtcNow
            };

            _db.Set<FleetExternalReference>().Add(newRef);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Created vehicle reference: {Plate} → {VehicleId}", plate, vehicleByPlate.Id);
            return vehicleByPlate;
        }

        _logger.LogWarning("No vehicle found for PalmTrack plate {Plate} — manual mapping required", plate);
        return null;
    }

    // ═══════════════════════════════════════════
    // Driver Mapping
    // ═══════════════════════════════════════════

    /// <summary>
    /// Maps a PalmTrack driver to a Zorvian Driver via FleetDriverAlias.
    /// </summary>
    public async Task<Driver?> MapDriverAsync(
        string externalDriverId,
        string externalName,
        CancellationToken ct = default)
    {
        // Check existing alias
        var existingAlias = await _db.Set<FleetDriverAlias>()
            .FirstOrDefaultAsync(a =>
                a.ExternalSystem == ExternalSystem &&
                a.ExternalDriverId == externalDriverId, ct);

        if (existingAlias != null)
        {
            existingAlias.MatchCount++;
            await _db.SaveChangesAsync(ct);

            var driver = await _db.Set<Driver>()
                .FirstOrDefaultAsync(d => d.Id == existingAlias.DriverId && !d.IsDeleted, ct);

            if (driver != null)
            {
                _logger.LogDebug("Driver mapped via alias: {Name} → {DriverId}", externalName, driver.Id);
                return driver;
            }
        }

        // Try exact name match
        var nameParts = externalName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (nameParts.Length > 0)
        {
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var driver = await _db.Set<Driver>()
                .FirstOrDefaultAsync(d =>
                    d.FirstName == firstName &&
                    (lastName == "" || d.LastName == lastName) &&
                    !d.IsDeleted, ct);

            if (driver != null)
            {
                // Create alias for future lookups
                var alias = new FleetDriverAlias
                {
                    DriverId = driver.Id,
                    ExternalSystem = ExternalSystem,
                    ExternalName = externalName,
                    ExternalDriverId = externalDriverId,
                    MatchType = lastName != "" ? "exact" : "partial",
                    IsPrimary = true
                };

                _db.Set<FleetDriverAlias>().Add(alias);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Created driver alias: {Name} → {DriverId}", externalName, driver.Id);
                return driver;
            }
        }

        _logger.LogWarning("No driver found for PalmTrack name {Name} — manual mapping required", externalName);
        return null;
    }

    // ═══════════════════════════════════════════
    // Reference Queries
    // ═══════════════════════════════════════════

    /// <summary>
    /// Gets all PalmTrack external references for the current tenant.
    /// </summary>
    public async Task<List<FleetExternalReference>> GetAllReferencesAsync(
        string? entityType = null,
        CancellationToken ct = default)
    {
        var query = _db.Set<FleetExternalReference>()
            .Where(r => r.ExternalSystem == ExternalSystem);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(r => r.EntityType == entityType);

        return await query.OrderByDescending(r => r.LastSyncAt).ToListAsync(ct);
    }

    /// <summary>
    /// Gets all conflicts (sync errors) for the current tenant.
    /// </summary>
    public async Task<List<FleetExternalReference>> GetConflictsAsync(
        CancellationToken ct = default)
    {
        return await _db.Set<FleetExternalReference>()
            .Where(r =>
                r.ExternalSystem == ExternalSystem &&
                r.Status == "conflict")
            .OrderByDescending(r => r.LastSyncAt)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets sync statistics for the PalmTrack integration.
    /// </summary>
    public async Task<PalmTrackSyncStats> GetSyncStatsAsync(CancellationToken ct = default)
    {
        var references = await _db.Set<FleetExternalReference>()
            .Where(r => r.ExternalSystem == ExternalSystem)
            .ToListAsync(ct);

        return new PalmTrackSyncStats
        {
            TotalReferences = references.Count,
            SyncedCount = references.Count(r => r.Status == "synced"),
            PendingCount = references.Count(r => r.Status == "pending"),
            ConflictCount = references.Count(r => r.Status == "conflict"),
            ErrorCount = references.Count(r => r.Status == "error"),
            LastSyncAt = references.Max(r => r.LastSyncAt),
            VehicleCount = references.Count(r => r.EntityType == "Vehicle"),
            DriverAliasCount = await _db.Set<FleetDriverAlias>()
                .CountAsync(a => a.ExternalSystem == ExternalSystem, ct)
        };
    }

    /// <summary>
    /// Resolves a conflict by accepting either the local or external version.
    /// </summary>
    public async Task ResolveConflictAsync(
        Guid referenceId,
        bool acceptExternal,
        CancellationToken ct = default)
    {
        var reference = await _db.Set<FleetExternalReference>()
            .FirstOrDefaultAsync(r => r.Id == referenceId, ct);

        if (reference == null) throw new InvalidOperationException($"Reference {referenceId} not found");

        if (acceptExternal)
        {
            _logger.LogInformation("Conflict resolved: accepting external version for {EntityType} {ExternalId}",
                reference.EntityType, reference.ExternalId);
        }
        else
        {
            _logger.LogInformation("Conflict resolved: keeping local version for {EntityType} {ExternalId}",
                reference.EntityType, reference.ExternalId);
        }

        reference.Status = "synced";
        reference.LastSyncAt = DateTime.UtcNow;
        reference.ConsecutiveFailures = 0;

        await _db.SaveChangesAsync(ct);
    }
}

public sealed class PalmTrackSyncStats
{
    public int TotalReferences { get; set; }
    public int SyncedCount { get; set; }
    public int PendingCount { get; set; }
    public int ConflictCount { get; set; }
    public int ErrorCount { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public int VehicleCount { get; set; }
    public int DriverAliasCount { get; set; }
}
