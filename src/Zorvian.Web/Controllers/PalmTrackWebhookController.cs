using System.Text.Json;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zorvian.Application.Messages;
using Zorvian.Web.Services.PalmTrack;
using Zorvian.Core.Entities;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Web.Controllers;

/// <summary>
/// Receives webhooks from PalmTrack.
/// Route: zorvian/v1/palm/webhooks (plan §2.1)
/// Validation delegated to IPalmTrackWebhookValidator (plan §2.3).
/// </summary>
[ApiController]
[Route("zorvian/v1/palm/webhooks")]
public sealed class PalmTrackWebhookController : ControllerBase
{
    private readonly IPalmTrackWebhookValidator _validator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ZorvianDbContext _db;
    private readonly ILogger<PalmTrackWebhookController> _logger;

    public PalmTrackWebhookController(
        IPalmTrackWebhookValidator validator,
        IPublishEndpoint publishEndpoint,
        ZorvianDbContext db,
        ILogger<PalmTrackWebhookController> logger)
    {
        _validator = validator;
        _publishEndpoint = publishEndpoint;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Receives webhook events from PalmTrack.
    /// Validation order per plan §2.2: Content-Type → Event → Idempotency → Signature → HMAC → Org.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook(CancellationToken ct)
    {
        // Read and parse body
        Request.EnableBuffering();
        string bodyString;
        using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            bodyString = await reader.ReadToEndAsync(ct);
        }
        Request.Body.Position = 0;

        JsonElement body;
        try
        {
            body = JsonSerializer.Deserialize<JsonElement>(bodyString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse webhook body");
            return StatusCode(500, new { error = "internal_error" });
        }

        // Delegate full validation to validator service (plan §2.3, §3)
        var validation = await _validator.ValidateAsync(Request, body);
        if (!validation.Success)
        {
            return validation.StatusCode switch
            {
                401 => Unauthorized(new { error = validation.Error }),
                404 => NotFound(new { error = validation.Error, @event = validation.Event }),
                409 => Conflict(new { error = validation.Error, idempotencyKey = validation.IdempotencyKey }),
                422 => UnprocessableEntity(new { error = validation.Error, organizationId = validation.OrganizationId }),
                _ => BadRequest(new { error = validation.Error })
            };
        }

        // Publish event to MassTransit for async processing
        var message = new PalmTrackWebhookReceived
        {
            Event = validation.Event!,
            OrganizationId = validation.OrganizationId!,
            IdempotencyKey = validation.IdempotencyKey!,
            Payload = body,
            ReceivedAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(message, ct);

        _logger.LogInformation(
            "PalmTrack webhook accepted: event={Event}, org={Org}, key={Key}",
            message.Event, message.OrganizationId, message.IdempotencyKey);

        return Ok(new { status = "accepted" });
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", integration = "palmtrack", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Lists failed webhook events in the DLQ (plan §9.2).
    /// </summary>
    [HttpGet("dlq")]
    public async Task<IActionResult> GetDlqEvents(
        [FromQuery] int limit = 50,
        [FromQuery] bool unresolvedOnly = true,
        CancellationToken ct = default)
    {
        var query = _db.Set<PalmTrackWebhookDlq>().AsQueryable();

        if (unresolvedOnly)
            query = query.Where(d => !d.IsResolved);

        var events = await query
            .OrderByDescending(d => d.FailedAt)
            .Take(limit)
            .Select(d => new
            {
                d.Id,
                d.IdempotencyKey,
                d.Event,
                d.OrganizationId,
                d.Error,
                d.FailedAt,
                d.RetryCount,
                d.IsResolved,
                d.ResolvedAt,
                d.ResolvedBy,
            })
            .ToListAsync(ct);

        return Ok(events);
    }

    /// <summary>
    /// Resolves a DLQ event (mark as resolved or retry).
    /// </summary>
    [HttpPost("dlq/{id}/resolve")]
    public async Task<IActionResult> ResolveDlqEvent(
        Guid id,
        [FromBody] ResolveDlqRequest request,
        CancellationToken ct)
    {
        var entry = await _db.Set<PalmTrackWebhookDlq>()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (entry == null)
            return NotFound(new { error = "dlq_entry_not_found" });

        entry.IsResolved = true;
        entry.ResolvedAt = DateTime.UtcNow;
        entry.ResolvedBy = request.ResolvedBy ?? "system";

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("DLQ event resolved: {Id}, by {User}", id, entry.ResolvedBy);

        return Ok(new { status = "resolved" });
    }
}

public sealed class ResolveDlqRequest
{
    public string? ResolvedBy { get; set; }
}
