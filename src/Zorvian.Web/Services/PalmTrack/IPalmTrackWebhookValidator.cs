using System.Text.Json;

namespace Zorvian.Web.Services.PalmTrack;

/// <summary>
/// Validates incoming PalmTrack webhooks: headers, HMAC signature, idempotency, organization.
/// Located in Web layer because HttpRequest is ASP.NET Core specific.
/// </summary>
public interface IPalmTrackWebhookValidator
{
    Task<ValidationResult> ValidateAsync(HttpRequest request, JsonElement body);
}

public sealed class ValidationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }
    public string? Event { get; init; }
    public string? OrganizationId { get; init; }
    public string? IdempotencyKey { get; init; }

    public static ValidationResult SuccessResult(string eventName, string organizationId, string idempotencyKey) =>
        new() { Success = true, Event = eventName, OrganizationId = organizationId, IdempotencyKey = idempotencyKey };

    public static ValidationResult Fail(string error, int statusCode, string? eventName = null, string? idempotencyKey = null, string? organizationId = null) =>
        new() { Success = false, Error = error, StatusCode = statusCode, Event = eventName, IdempotencyKey = idempotencyKey, OrganizationId = organizationId };
}
