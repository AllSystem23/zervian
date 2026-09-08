using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Messages;
using Zorvian.Core.Entities;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Messaging.Consumers;

/// <summary>
/// Processes failed PalmTrack webhook events that exhausted retries.
/// Persists them to the PalmTrackWebhookDlq table for manual review.
/// </summary>
public sealed class PalmTrackWebhookDlqConsumer : IConsumer<Fault<PalmTrackWebhookReceived>>
{
    private readonly ZorvianDbContext _db;
    private readonly ILogger<PalmTrackWebhookDlqConsumer> _logger;

    public PalmTrackWebhookDlqConsumer(ZorvianDbContext db, ILogger<PalmTrackWebhookDlqConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Fault<PalmTrackWebhookReceived>> context)
    {
        var original = context.Message.Message;
        var fault = context.Message;

        var errorMessages = fault.Exceptions
            .Select(e => e.Message)
            .ToList();

        var dlqEntry = new PalmTrackWebhookDlq
        {
            IdempotencyKey = original.IdempotencyKey,
            Event = original.Event,
            OrganizationId = original.OrganizationId,
            Payload = original.Payload.GetRawText(),
            Error = errorMessages.FirstOrDefault() ?? "Unknown error",
            FailedAt = DateTime.UtcNow,
            RetryCount = 3,
            IsResolved = false
        };

        _db.Set<PalmTrackWebhookDlq>().Add(dlqEntry);

        // Also update the webhook log status to dlq
        var log = await _db.Set<PalmTrackWebhookLog>()
            .FirstOrDefaultAsync(l => l.IdempotencyKey == original.IdempotencyKey);

        if (log != null)
        {
            log.Status = "dlq";
            log.Error = dlqEntry.Error;
        }

        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogError(
            "PalmTrack webhook moved to DLQ: event={Event}, key={Key}, error={Error}",
            original.Event, original.IdempotencyKey, dlqEntry.Error);
    }
}
