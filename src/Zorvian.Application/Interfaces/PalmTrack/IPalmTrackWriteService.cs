namespace Zorvian.Application.Interfaces.PalmTrack;

/// <summary>
/// Plan Paso 5 §2.2 — Write channel from Zorvian to PalmTrack.
/// Calls PalmTrack write API endpoints (/api/palm/v1/write/*).
/// 
/// Entities enabled for writing (plan §2.2.2):
/// - vehicles: fleet sync (Opción A/C)
/// - users: role propagation
/// - settings: shared config (timezone, currency)
/// - inventory: stock adjustments (per-org flag)
/// </summary>
public interface IPalmTrackWriteService
{
    /// <summary>
    /// Write vehicle data to PalmTrack.
    /// POST /api/palm/v1/write/vehicles
    /// </summary>
    Task<WriteResult> WriteVehicleAsync(
        string palmDocId,
        string organizationId,
        string? status = null,
        decimal? mileage = null,
        string? notes = null,
        string? baseUpdatedAt = null);

    /// <summary>
    /// Write user role/assignment to PalmTrack.
    /// POST /api/palm/v1/write/users
    /// </summary>
    Task<WriteResult> WriteUserAsync(
        string firebaseUid,
        string? role = null,
        List<string>? assignedFarms = null);

    /// <summary>
    /// Write shared settings to PalmTrack.
    /// POST /api/palm/v1/write/settings
    /// </summary>
    Task<WriteResult> WriteSettingsAsync(
        string organizationId,
        string? timezone = null,
        string? currency = null,
        string? language = null);

    /// <summary>
    /// Adjust inventory stock in PalmTrack.
    /// POST /api/palm/v1/write/inventory
    /// </summary>
    Task<WriteResult> WriteInventoryAdjustmentAsync(
        string itemId,
        decimal quantity,
        string reason,
        decimal? unitCost = null);
}

/// <summary>
/// Result of a write operation to PalmTrack.
/// </summary>
public sealed class WriteResult
{
    public bool Success { get; init; }
    public int? HttpStatusCode { get; init; }
    public string? Error { get; init; }
    public string? IdempotencyKey { get; init; }
    public bool IsConflict => HttpStatusCode == 409;

    public static WriteResult Ok(string idempotencyKey) => new()
    {
        Success = true,
        HttpStatusCode = 200,
        IdempotencyKey = idempotencyKey,
    };

    public static WriteResult Conflict(string idempotencyKey) => new()
    {
        Success = false,
        HttpStatusCode = 409,
        Error = "conflict",
        IdempotencyKey = idempotencyKey,
    };

    public static WriteResult Failed(int statusCode, string error, string? idempotencyKey = null) => new()
    {
        Success = false,
        HttpStatusCode = statusCode,
        Error = error,
        IdempotencyKey = idempotencyKey,
    };
}
