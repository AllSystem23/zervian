using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Core.Entities;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services.PalmTrack;

/// <summary>
/// Manages idempotency for PalmTrack webhook events using PalmTrackWebhookLog.
/// </summary>
public sealed class PalmTrackIdempotencyService : IPalmTrackIdempotencyService
{
    private readonly ZorvianDbContext _db;
    private readonly ILogger<PalmTrackIdempotencyService> _logger;

    public PalmTrackIdempotencyService(ZorvianDbContext db, ILogger<PalmTrackIdempotencyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> IsProcessedAsync(string idempotencyKey)
    {
        return await _db.Set<PalmTrackWebhookLog>()
            .AnyAsync(l =>
                l.IdempotencyKey == idempotencyKey &&
                l.Status != "failed");
    }

    public async Task MarkProcessedAsync(string idempotencyKey, string eventName, string organizationId, string? payload = null)
    {
        var log = new PalmTrackWebhookLog
        {
            IdempotencyKey = idempotencyKey,
            Event = eventName,
            OrganizationId = organizationId,
            Payload = payload,
            Status = "processed",
            ReceivedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };

        _db.Set<PalmTrackWebhookLog>().Add(log);
        await _db.SaveChangesAsync();

        _logger.LogDebug("Idempotency key marked processed: {Key}", idempotencyKey);
    }

    public async Task MarkFailedAsync(string idempotencyKey, string error)
    {
        var log = await _db.Set<PalmTrackWebhookLog>()
            .FirstOrDefaultAsync(l => l.IdempotencyKey == idempotencyKey);

        if (log != null)
        {
            log.Status = "failed";
            log.Error = error;
            log.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
