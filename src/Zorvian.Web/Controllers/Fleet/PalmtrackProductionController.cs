using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zorvian.Core.Entities;
using Zorvian.Core.Entities.Fleet;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;
using Zorvian.Web.Authorization;

namespace Zorvian.Web.Controllers.Fleet;

[ApiController]
[Authorize]
[Route("zorvian/v1/palmtrack/production")]
public sealed class PalmtrackProductionController : ControllerBase
{
    private readonly ZorvianDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ILogger<PalmtrackProductionController> _logger;

    public PalmtrackProductionController(
        ZorvianDbContext db,
        ITenantContext tenant,
        ILogger<PalmtrackProductionController> logger)
    {
        _db = db;
        _tenant = tenant;
        _logger = logger;
    }

    /// <summary>
    /// GET /palmtrack/production/summary
    /// Retorna resumen agregado de producción agrícola desde ExternalReferences
    /// sincronizados desde PalmTrack (entityType = "production_log").
    ///
    /// Query params opcionales:
    /// - startDate: fecha desde (YYYY-MM-DD)
    /// - endDate: fecha hasta (YYYY-MM-DD)
    /// </summary>
    [HttpGet("summary")]
    [RequirePermission(Permissions.FleetRead)]
    public async Task<IActionResult> GetProductionSummary(
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
    {
        var tenantId = _tenant.TenantId.Value;
        if (tenantId == Guid.Empty) return Unauthorized();

        var startFilter = ParseDate(startDate);
        var endFilter = ParseDate(endDate);

        var productionLogs = await _db.Set<FleetExternalReference>()
            .Where(r => r.TenantId == tenantId.ToString()
                && r.EntityType == "production_log"
                && r.Status == "synced"
                && (!startFilter.HasValue || r.CreatedAt >= startFilter.Value)
                && (!endFilter.HasValue || r.CreatedAt <= endFilter.Value))
            .ToListAsync();

        var totalBunches = 0L;
        var totalWeight = 0m;
        var totalBags = 0L;

        var farmNames = new HashSet<string>();
        var lotNames = new HashSet<string>();

        foreach (var log in productionLogs)
        {
            var payload = ParsePayload(log.ExternalPayload);
            totalBunches += GetInt64(payload, "bunchCount");
            totalWeight += GetDecimal(payload, "racimoWeight");
            totalBags += GetInt64(payload, "bagCount");

            var farmName = GetString(payload, "farmName");
            if (!string.IsNullOrEmpty(farmName))
                farmNames.Add(farmName);

            var lotName = GetString(payload, "lotName");
            if (!string.IsNullOrEmpty(lotName))
                lotNames.Add(lotName);
        }

        var avgWeight = totalBunches > 0 ? totalWeight / totalBunches : 0m;
        var avgBunchWeight = totalBunches > 0 ? totalWeight / totalBunches : 0m;

        var producerCount = await _db.Set<Employee>()
            .Where(e => e.TenantId == tenantId.ToString() && e.Status == "active")
            .CountAsync();

        // Daily trend
        var dailyTrend = productionLogs
            .GroupBy(r => r.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                long bunches = 0;
                decimal weight = 0m;
                foreach (var r in g)
                {
                    var p = ParsePayload(r.ExternalPayload);
                    bunches += GetInt64(p, "bunchCount");
                    weight += GetDecimal(p, "racimoWeight");
                }
                return new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    bunches,
                    weight = Math.Round(weight, 2),
                };
            })
            .ToList();

        return Ok(new
        {
            totalBunches,
            totalWeight,
            avgWeight,
            totalBags,
            avgBunchWeight,
            activeFarms = farmNames.Count,
            activeLots = lotNames.Count,
            totalProducers = producerCount,
            byFarm = new List<object>(),
            dailyTrend,
            generatedAt = DateTime.UtcNow,
        });
    }

    private static DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;
        if (DateOnly.TryParse(dateStr, out var d))
            return d.ToDateTime(TimeOnly.MinValue);
        return null;
    }

    private static Dictionary<string, object> ParsePayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            return new Dictionary<string, object>();

        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            var result = new Dictionary<string, object>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var v = prop.Value;
                result[prop.Name] = v.ValueKind switch
                {
                    JsonValueKind.String => v.GetString() ?? "",
                    JsonValueKind.Number => v.Deserialize<object>()!,
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null!,
                    _ => v.ToString(),
                };
            }
            return result;
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static long GetInt64(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var val))
            return 0;
        return val switch
        {
            long l => l,
            int i => i,
            double d => (long)d,
            decimal m => (long)m,
            _ => 0,
        };
    }

    private static decimal GetDecimal(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var val))
            return 0m;
        return val switch
        {
            decimal m => m,
            double d => (decimal)d,
            long l => l,
            int i => i,
            _ => 0m,
        };
    }

    private static string? GetString(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var val))
            return null;
        return val?.ToString();
    }


}
