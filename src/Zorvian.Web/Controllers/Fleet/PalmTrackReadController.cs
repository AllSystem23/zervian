using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zorvian.Web.Authorization;

namespace Zorvian.Web.Controllers.Fleet;

/// <summary>
/// Controller proxy para lectura de entidades PalmTrack.
/// Consume la API de lectura de PalmTrack (/api/palm/v1/*) y la expone
/// al frontend de Zorvian con los DTOs esperados.
///
/// Los endpoints disponibles dependen de la configuración de PalmTrack:ReadApiBaseUrl
/// y PalmTrack:ReadApiKey en appsettings.json.
///
/// Si la API de PalmTrack no está configurada, los endpoints retornan 503
/// con un mensaje descriptivo.
/// </summary>
[ApiController]
[Authorize]
[Route("zorvian/v1/palmtrack")]
public sealed class PalmTrackReadController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PalmTrackReadController> _logger;

    public PalmTrackReadController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PalmTrackReadController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    private string? ReadApiBaseUrl =>
        _configuration["PalmTrack:ReadApiBaseUrl"];

    private string? ReadApiKey =>
        _configuration["PalmTrack:ReadApiKey"];

    private static readonly Uri[] AllowedPalmTrackHosts = new Uri[]
    {
        new Uri("https://palmtrack.app"),
        new Uri("https://api.palmtrack.app"),
    };

    private bool IsAllowedPalmTrackUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            // Solo HTTPS permitido
            if (uri.Scheme != Uri.UriSchemeHttps)
                return false;
            // Validar que el host esté en la lista permitida
            foreach (var allowed in AllowedPalmTrackHosts)
            {
                if (uri.Host == allowed.Host)
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool IsReadApiConfigured =>
        !string.IsNullOrWhiteSpace(ReadApiBaseUrl);

    // ═══════════════════════════════════════════
    // Farms
    // ═══════════════════════════════════════════

    /// <summary>
    /// GET /palmtrack/farms
    /// Lista las fincas de la organización en PalmTrack.
    /// </summary>
    [HttpGet("farms")]
    [RequirePermission(Permissions.FleetRead)]
    public async Task<IActionResult> GetFarms(
        [FromQuery] int limit = 100,
        [FromQuery] string? startAfter = null)
    {
        return await ProxyGetAsync<PalmTrackFarmResponse>(
            "farms",
            new Dictionary<string, object>
            {
                ["limit"] = limit,
                ["startAfter"] = startAfter ?? "",
            });
    }

    // ═══════════════════════════════════════════
    // Lots
    // ═══════════════════════════════════════════

    /// <summary>
    /// GET /palmtrack/lots
    /// Lista los lotes de la organización en PalmTrack.
    /// </summary>
    [HttpGet("lots")]
    [RequirePermission(Permissions.FleetRead)]
    public async Task<IActionResult> GetLots(
        [FromQuery] int limit = 100,
        [FromQuery] string? startAfter = null,
        [FromQuery] string? farmId = null)
    {
        var paramsDict = new Dictionary<string, object>
        {
            ["limit"] = limit,
            ["startAfter"] = startAfter ?? "",
        };
        if (!string.IsNullOrWhiteSpace(farmId))
            paramsDict["farmId"] = farmId;

        return await ProxyGetAsync<PalmTrackLotResponse>(
            "lots",
            paramsDict);
    }

    // ═══════════════════════════════════════════
    // Producers
    // ═══════════════════════════════════════════

    /// <summary>
    /// GET /palmtrack/producers
    /// Lista los productores de la organización en PalmTrack.
    /// </summary>
    [HttpGet("producers")]
    [RequirePermission(Permissions.FleetRead)]
    public async Task<IActionResult> GetProducers(
        [FromQuery] int limit = 100,
        [FromQuery] string? startAfter = null,
        [FromQuery] string? search = null)
    {
        var paramsDict = new Dictionary<string, object>
        {
            ["limit"] = limit,
            ["startAfter"] = startAfter ?? "",
        };
        if (!string.IsNullOrWhiteSpace(search))
            paramsDict["search"] = search;

        return await ProxyGetAsync<PalmTrackProducerResponse>(
            "producers",
            paramsDict);
    }

    // ═══════════════════════════════════════════
    // Sales Logs
    // ═══════════════════════════════════════════

    /// <summary>
    /// GET /palmtrack/sales-logs
    /// Lista los registros de ventas de la organización en PalmTrack.
    /// </summary>
    [HttpGet("sales-logs")]
    [RequirePermission(Permissions.SaleRead)]
    public async Task<IActionResult> GetSalesLogs(
        [FromQuery] int limit = 100,
        [FromQuery] string? startAfter = null,
        [FromQuery] string? producerCode = null,
        [FromQuery] string? since = null)
    {
        var paramsDict = new Dictionary<string, object>
        {
            ["limit"] = limit,
            ["startAfter"] = startAfter ?? "",
        };
        if (!string.IsNullOrWhiteSpace(producerCode))
            paramsDict["producerCode"] = producerCode;
        if (!string.IsNullOrWhiteSpace(since))
            paramsDict["since"] = since;

        return await ProxyGetAsync<PalmTrackSalesLogResponse>(
            "sales-logs",
            paramsDict);
    }

    // ═══════════════════════════════════════════
    // Inventory
    // ═══════════════════════════════════════════

    /// <summary>
    /// GET /palmtrack/inventory
    /// Lista los items de inventario de la organización en PalmTrack.
    /// </summary>
    [HttpGet("inventory")]
    [RequirePermission(Permissions.InventoryRead)]
    public async Task<IActionResult> GetInventory(
        [FromQuery] int limit = 100,
        [FromQuery] string? startAfter = null,
        [FromQuery] string? producerCode = null)
    {
        var paramsDict = new Dictionary<string, object>
        {
            ["limit"] = limit,
            ["startAfter"] = startAfter ?? "",
        };
        if (!string.IsNullOrWhiteSpace(producerCode))
            paramsDict["producerCode"] = producerCode;

        return await ProxyGetAsync<PalmTrackInventoryResponse>(
            "inventory",
            paramsDict);
    }

    // ═══════════════════════════════════════════
    // Proxy helper
    // ═══════════════════════════════════════════

    private async Task<IActionResult> ProxyGetAsync<TResponse>(
        string endpoint,
        Dictionary<string, object> queryParams)
        where TResponse : class
    {
        if (!IsReadApiConfigured || !IsAllowedPalmTrackUrl(ReadApiBaseUrl!))
        {
            _logger.LogWarning(
                "PalmTrack Read API not configured or URL not allowed. Endpoint={Endpoint}, Url={Url}",
                endpoint, ReadApiBaseUrl);
            return StatusCode(503, new
            {
                error = "PalmTrack Read API not configured",
                message = "Contact your administrator to configure PalmTrack:ReadApiBaseUrl and PalmTrack:ReadApiKey",
                detail = !IsReadApiConfigured
                    ? "PalmTrack:ReadApiBaseUrl is not configured"
                    : "PalmTrack:ReadApiBaseUrl is not a whitelisted PalmTrack URL",
            });
        }

        if (string.IsNullOrWhiteSpace(ReadApiKey))
        {
            _logger.LogWarning(
                "PalmTrack Read API key is empty. Endpoint={Endpoint}",
                endpoint);
            return StatusCode(503, new
            {
                error = "PalmTrack Read API not configured",
                message = "Contact your administrator to configure PalmTrack:ReadApiKey",
            });
        }

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(ReadApiBaseUrl!);
            client.Timeout = TimeSpan.FromSeconds(15);

            // Build query string
            var queryParts = queryParams
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value?.ToString()))
                .Select(kv =>
                    $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}")
                .ToList();

            var uri = queryParts.Count > 0
                ? $"{endpoint}?{string.Join("&", queryParts)}"
                : endpoint;

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.Headers.Add("X-PalmTrack-API-Key", ReadApiKey!);
            request.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "PalmTrack Read API error: endpoint={Endpoint}, status={Status}, body={Body}",
                    endpoint, response.StatusCode, body);

                return StatusCode((int)response.StatusCode, new
                {
                    error = "PalmTrack Read API error",
                    message = TryExtractErrorMessage(body),
                    rawStatus = response.StatusCode,
                });
            }

            // Parse response
            List<JsonElement> itemsList;
            JsonElement? paginationEl = null;
            JsonElement? metaEl = null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // PalmTrack API returns { success, data, pagination, meta }
                // Mapeamos al formato esperado por el frontend
                if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
                {
                    itemsList = new List<JsonElement>();
                    foreach (var item in dataEl.EnumerateArray()) itemsList.Add(item);
                }
                else if (root.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
                {
                    itemsList = new List<JsonElement>();
                    foreach (var item in itemsEl.EnumerateArray()) itemsList.Add(item);
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    itemsList = new List<JsonElement>();
                    foreach (var item in root.EnumerateArray()) itemsList.Add(item);
                }
                else
                {
                    _logger.LogWarning(
                        "PalmTrack Read API returned unexpected response format for endpoint={Endpoint}. RootKind={RootKind}, Body={Body}",
                        endpoint, root.ValueKind, body.Length > 500 ? body.Substring(0, 500) : body);

                    return StatusCode(502, new
                    {
                        error = "Bad Gateway",
                        message = "PalmTrack Read API returned an unexpected response format",
                        detail = $"Expected array or {{data/items: [...]}} but got {root.ValueKind}",
                    });
                }

                if (root.TryGetProperty("pagination", out var pagProp))
                    paginationEl = pagProp;
                if (root.TryGetProperty("meta", out var metaProp))
                    metaEl = metaProp;
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex,
                    "Invalid JSON from PalmTrack Read API: endpoint={Endpoint}, body={Body}",
                    endpoint, body.Length > 500 ? body.Substring(0, 500) : body);

                return StatusCode(502, new
                {
                    error = "Bad Gateway",
                    message = "PalmTrack Read API returned invalid JSON",
                    detail = jex.Message,
                });
            }

            var result = (object)new
            {
                items = itemsList,
                pagination = paginationEl,
                meta = metaEl,
            };

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Network error proxying to PalmTrack Read API: endpoint={Endpoint}, url={Url}",
                endpoint, ReadApiBaseUrl);
            return StatusCode(502, new
            {
                error = "Bad Gateway",
                message = "Cannot reach PalmTrack Read API",
                detail = "Check that PalmTrack:ReadApiBaseUrl is reachable from this server",
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "Timeout proxying to PalmTrack Read API: endpoint={Endpoint}, url={Url}",
                endpoint, ReadApiBaseUrl);
            return StatusCode(504, new
            {
                error = "Gateway Timeout",
                message = "PalmTrack Read API did not respond in time",
                detail = "Check PalmTrack:ReadApiBaseUrl and network connectivity",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error proxying to PalmTrack Read API: endpoint={Endpoint}, url={Url}",
                endpoint, ReadApiBaseUrl);
            return StatusCode(500, new
            {
                error = "Internal error",
                message = "Error communicating with PalmTrack Read API",
                detail = "Check server logs for details",
            });
        }
    }

    private static string TryExtractErrorMessage(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString() ?? body;
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString() ?? body;
        }
        catch { }
        return body.Length > 200 ? body.Substring(0, 200) : body;
    }
}

// ─────────────────────────────────────────────
// DTOs de respuesta (simplify — frontend espera formato específico)
// ─────────────────────────────────────────────

/// <summary>
/// Response para lista de farms.
/// </summary>
public sealed record PalmTrackFarmResponse(
    List<PalmTrackFarmDto> Items,
    PalmTrackPagination? Pagination,
    PalmTrackMeta? Meta
);

public sealed record PalmTrackFarmDto(
    string Id,
    string Name,
    string? Location,
    int TotalLots,
    int ActiveLots,
    string OrganizationId
);

/// <summary>
/// Response para lista de lots.
/// </summary>
public sealed record PalmTrackLotResponse(
    List<PalmTrackLotDto> Items,
    PalmTrackPagination? Pagination,
    PalmTrackMeta? Meta
);

public sealed record PalmTrackLotDto(
    string Id,
    string Name,
    string? FarmId,
    string? FarmName,
    string? CropVariety,
    int? AreaHectares,
    string OrganizationId
);

/// <summary>
/// Response para lista de producers.
/// </summary>
public sealed record PalmTrackProducerResponse(
    List<PalmTrackProducerDto> Items,
    PalmTrackPagination? Pagination,
    PalmTrackMeta? Meta
);

public sealed record PalmTrackProducerDto(
    string Id,
    string Name,
    string? Code,
    string? Phone,
    string? Email,
    int TotalFarms,
    string OrganizationId
);

/// <summary>
/// Response para lista de sales-logs.
/// </summary>
public sealed record PalmTrackSalesLogResponse(
    List<PalmTrackSalesLogDto> Items,
    PalmTrackPagination? Pagination,
    PalmTrackMeta? Meta
);

public sealed record PalmTrackSalesLogDto(
    string Id,
    string? ClientName,
    string? ProductName,
    double Quantity,
    double UnitPrice,
    double TotalAmount,
    string? Currency,
    string? PaymentMethod,
    string? PaymentStatus,
    string Date,
    string OrganizationId
);

/// <summary>
/// Response para lista de inventory.
/// </summary>
public sealed record PalmTrackInventoryResponse(
    List<PalmTrackInventoryDto> Items,
    PalmTrackPagination? Pagination,
    PalmTrackMeta? Meta
);

public sealed record PalmTrackInventoryDto(
    string Id,
    string? ProductName,
    string? Code,
    string? Unit,
    double Stock,
    double UnitCost,
    string OrganizationId
);

/// <summary>
/// Pagination info desde PalmTrack API.
/// </summary>
public sealed record PalmTrackPagination(
    int Limit,
    bool HasMore,
    string? NextCursor
);

/// <summary>
/// Meta info desde PalmTrack API.
/// </summary>
public sealed record PalmTrackMeta(
    string OrgId,
    string Source,
    string ApiVersion,
    DateTime GeneratedAt
);
