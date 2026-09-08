using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces.PalmTrack;

namespace Zorvian.Web.Services.PalmTrack;

/// <summary>
/// Validates incoming PalmTrack webhooks per plan §3.
/// Checks Content-Type, X-Webhook-Event, X-Idempotency-Key, X-Webhook-Signature,
/// HMAC-SHA256, idempotency, and organization reconciliation.
/// </summary>
public sealed class PalmTrackWebhookValidator : IPalmTrackWebhookValidator
{
    private readonly IPalmTrackSecretService _secretService;
    private readonly IPalmTrackIdempotencyService _idempotencyService;
    private readonly IPalmTrackIdentityService _identityService;
    private readonly ILogger<PalmTrackWebhookValidator> _logger;

    private static readonly HashSet<string> AllowedEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "sale.created", "production.logged",
        "inventory.updated", "inventory.entry.created", "inventory.exit.created",
        "expense.created", "labor_log.created", "collaborator.created",
        "seedbed.created", "seedling.created", "transplant.created",
        "irrigation_log.created", "harvest_audit.created", "quality_audit.created",
        "daily_observation.created", "unit_of_measure.updated", "crop_variety.updated",
        "vehicle.created", "vehicle.updated",
        "trip.created", "trip.updated",
        "fuel_log.created", "machinery.created", "maintenance_log.created",
    };

    public PalmTrackWebhookValidator(
        IPalmTrackSecretService secretService,
        IPalmTrackIdempotencyService idempotencyService,
        IPalmTrackIdentityService identityService,
        ILogger<PalmTrackWebhookValidator> logger)
    {
        _secretService = secretService;
        _idempotencyService = idempotencyService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateAsync(HttpRequest request, JsonElement body)
    {
        if (!request.ContentType?.Contains("application/json") ?? true)
            return ValidationResult.Fail("invalid_content_type", 400);

        var eventName = request.Headers["X-Webhook-Event"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(eventName) || !AllowedEvents.Contains(eventName))
            return ValidationResult.Fail("event_not_allowed", 404, eventName: eventName);

        var idempotencyKey = request.Headers["X-Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 256)
            return ValidationResult.Fail("missing_x_idempotency_key", 400);

        var signature = request.Headers["X-Webhook-Signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
            return ValidationResult.Fail("missing_signature", 401);

        if (!body.TryGetProperty("organizationId", out var orgIdElement) ||
            orgIdElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(orgIdElement.GetString()))
            return ValidationResult.Fail("missing_organization_id", 422);
        var organizationId = orgIdElement.GetString()!;

        var bodyString = await ReadBodyAsync(request);
        var matchingSecret = await _secretService.FindMatchingSecretAsync(organizationId, signature, bodyString);
        if (matchingSecret == null)
        {
            _logger.LogWarning("HMAC failed for org {OrgId}", organizationId);
            return ValidationResult.Fail("invalid_signature", 401);
        }

        var isDuplicate = await _idempotencyService.IsProcessedAsync(idempotencyKey);
        if (isDuplicate)
            return ValidationResult.Fail("duplicate_event", 409, idempotencyKey: idempotencyKey);

        var isReconciled = await _identityService.IsOrganizationReconciledAsync(organizationId);
        if (!isReconciled)
            return ValidationResult.Fail("unmapped_organization", 422, organizationId: organizationId);

        return ValidationResult.SuccessResult(eventName, organizationId, idempotencyKey);
    }

    private static async Task<string> ReadBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }
}
