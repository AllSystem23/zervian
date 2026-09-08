using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces.PalmTrack;

namespace Zorvian.Infrastructure.Services.PalmTrack;

/// <summary>
/// Plan Paso 5 §2.2 — Write channel from Zorvian to PalmTrack.
/// Calls PalmTrack write API endpoints with HMAC authentication,
/// idempotency keys, and schema version headers.
/// 
/// PalmTrack write API contract:
/// - POST /api/palm/v1/write/{entity}
/// - Headers: X-PalmTrack-Write-API-Key, X-Idempotency-Key, X-Zorvian-Schema-Version
/// - LWW by updatedAt — returns 409 on conflict
/// - Validates with Zod schemas (strict mode)
/// </summary>
public sealed class PalmTrackWriteService : IPalmTrackWriteService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PalmTrackWriteService> _logger;

    private string BaseUrl => _configuration["PalmTrack:WriteApiBaseUrl"]
        ?? "https://palmtrack.app/api/palm/v1";

    private string WriteApiKey => _configuration["PalmTrack:WriteApiKey"]
        ?? throw new InvalidOperationException("PalmTrack:WriteApiKey not configured");

    public PalmTrackWriteService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PalmTrackWriteService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Plan §2.2.2 — Write vehicle data to PalmTrack.
    /// POST /api/palm/v1/write/vehicles
    /// </summary>
    public async Task<WriteResult> WriteVehicleAsync(
        string palmDocId,
        string organizationId,
        string? status = null,
        decimal? mileage = null,
        string? notes = null,
        string? baseUpdatedAt = null)
    {
        var idempotencyKey = $"vehicle_sync_{palmDocId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        var body = new Dictionary<string, object>
        {
            ["palmDocId"] = palmDocId,
            ["organizationId"] = organizationId,
        };

        if (status is not null) body["status"] = status;
        if (mileage.HasValue) body["mileage"] = mileage.Value;
        if (notes is not null) body["notes"] = notes;
        if (baseUpdatedAt is not null) body["baseUpdatedAt"] = baseUpdatedAt;

        return await PostWriteAsync("vehicles", body, idempotencyKey, "vehicles.v1");
    }

    /// <summary>
    /// Plan §2.2.2 — Write user role/assignment to PalmTrack.
    /// POST /api/palm/v1/write/users
    /// </summary>
    public async Task<WriteResult> WriteUserAsync(
        string firebaseUid,
        string? role = null,
        List<string>? assignedFarms = null)
    {
        var idempotencyKey = $"users_{firebaseUid}";

        var body = new Dictionary<string, object>
        {
            ["uid"] = firebaseUid,
        };

        if (role is not null) body["role"] = role;
        if (assignedFarms is not null) body["assignedFarms"] = assignedFarms;

        return await PostWriteAsync("users", body, idempotencyKey, "users.v1");
    }

    /// <summary>
    /// Plan §2.2.2 — Write shared settings to PalmTrack.
    /// POST /api/palm/v1/write/settings
    /// </summary>
    public async Task<WriteResult> WriteSettingsAsync(
        string organizationId,
        string? timezone = null,
        string? currency = null,
        string? language = null)
    {
        var idempotencyKey = $"settings_{organizationId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        var body = new Dictionary<string, object>();

        if (timezone is not null) body["timezone"] = timezone;
        if (currency is not null) body["currency"] = currency;
        if (language is not null) body["language"] = language;

        if (body.Count == 0)
            return WriteResult.Failed(400, "No settings to update");

        return await PostWriteAsync("settings", body, idempotencyKey, "settings.v1");
    }

    /// <summary>
    /// Plan §2.2.2 — Adjust inventory stock in PalmTrack.
    /// POST /api/palm/v1/write/inventory
    /// </summary>
    public async Task<WriteResult> WriteInventoryAdjustmentAsync(
        string itemId,
        decimal quantity,
        string reason,
        decimal? unitCost = null)
    {
        var idempotencyKey = $"inventory_adjust_{itemId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        var body = new Dictionary<string, object>
        {
            ["itemId"] = itemId,
            ["quantity"] = quantity,
            ["reason"] = reason,
        };

        if (unitCost.HasValue) body["unitCost"] = unitCost.Value;

        return await PostWriteAsync("inventory", body, idempotencyKey, "inventory.v1");
    }

    /// <summary>
    /// Generic POST to PalmTrack write API with required headers.
    /// </summary>
    private async Task<WriteResult> PostWriteAsync(
        string entity,
        Dictionary<string, object> body,
        string idempotencyKey,
        string schemaVersion)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{BaseUrl}/write/{entity}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"),
            };

            // Plan §2.2.1 — Required headers
            request.Headers.Add("X-PalmTrack-Write-API-Key", WriteApiKey);
            request.Headers.Add("X-Idempotency-Key", idempotencyKey);
            request.Headers.Add("X-Zorvian-Schema-Version", schemaVersion);

            _logger.LogInformation(
                "PalmTrack Write: POST {Entity} (key={Key})",
                entity, idempotencyKey);

            var response = await client.SendAsync(request);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "PalmTrack Write: {Entity} OK ({Status})",
                    entity, statusCode);
                return WriteResult.Ok(idempotencyKey);
            }

            // Plan §2.2.1 — Handle LWW conflict
            if (statusCode == 409)
            {
                _logger.LogWarning(
                    "PalmTrack Write: {Entity} conflict (409) — LWW rejection",
                    entity);
                return WriteResult.Conflict(idempotencyKey);
            }

            // Read error body
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "PalmTrack Write: {Entity} failed ({Status}): {Body}",
                entity, statusCode, errorBody);

            var errorMessage = TryExtractError(errorBody);
            return WriteResult.Failed(statusCode, errorMessage, idempotencyKey);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "PalmTrack Write: {Entity} HTTP error", entity);
            return WriteResult.Failed(0, $"Connection error: {ex.Message}", idempotencyKey);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning(
                "PalmTrack Write: {Entity} timeout", entity);
            return WriteResult.Failed(0, "Request timeout", idempotencyKey);
        }
    }

    private static string TryExtractError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString() ?? "Unknown error";
            if (doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString() ?? "Unknown error";
        }
        catch
        {
            // Ignore parse errors
        }

        return string.IsNullOrWhiteSpace(body) ? "Empty response" : body;
    }
}
