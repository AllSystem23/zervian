using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Web.Services.PalmTrack;

namespace Zorvian.Tests.PalmTrack;

/// <summary>
/// Tests for PalmTrackWebhookValidator per plan §13.
/// </summary>
public sealed class PalmTrackWebhookValidatorTests
{
    private readonly Mock<IPalmTrackSecretService> _secretService = new();
    private readonly Mock<IPalmTrackIdempotencyService> _idempotencyService = new();
    private readonly Mock<IPalmTrackIdentityService> _identityService = new();
    private readonly Mock<ILogger<PalmTrackWebhookValidator>> _logger = new();
    private readonly PalmTrackWebhookValidator _sut;

    public PalmTrackWebhookValidatorTests()
    {
        _sut = new PalmTrackWebhookValidator(
            _secretService.Object,
            _idempotencyService.Object,
            _identityService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task ValidateAsync_MissingContentType_ShouldFail()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "text/plain";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

        var body = JsonDocument.Parse("{}").RootElement;
        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("invalid_content_type");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ValidateAsync_EventNotAllowed_ShouldFail404()
    {
        var context = CreateContext("bad.event", "key-1", "sig", "{}");

        var body = JsonDocument.Parse("{}").RootElement;
        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("event_not_allowed");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ValidateAsync_MissingIdempotencyKey_ShouldFail400()
    {
        var context = CreateContext("sale.created", "", "sig", "{}");

        var body = JsonDocument.Parse("{}").RootElement;
        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("missing_x_idempotency_key");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ValidateAsync_MissingSignature_ShouldFail401()
    {
        var context = CreateContext("sale.created", "key-1", "", "{}");

        var body = JsonDocument.Parse("{}").RootElement;
        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("missing_signature");
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ValidateAsync_MissingOrganizationId_ShouldFail422()
    {
        var payload = "{}";
        var context = CreateContext("sale.created", "key-1", "sig", payload);
        var body = JsonDocument.Parse(payload).RootElement;

        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("missing_organization_id");
        result.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task ValidateAsync_InvalidSignature_ShouldFail401()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });
        var context = CreateContext("sale.created", "key-1", "bad-sig", payload);
        var body = JsonDocument.Parse(payload).RootElement;

        _secretService.Setup(s => s.FindMatchingSecretAsync("org-123", "bad-sig", It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("invalid_signature");
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ValidateAsync_DuplicateEvent_ShouldFail409()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });
        var context = CreateContext("sale.created", "dup-key", "sig", payload);
        var body = JsonDocument.Parse(payload).RootElement;

        _secretService.Setup(s => s.FindMatchingSecretAsync("org-123", "sig", It.IsAny<string>()))
            .ReturnsAsync("the-secret");
        _idempotencyService.Setup(i => i.IsProcessedAsync("dup-key"))
            .ReturnsAsync(true);

        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("duplicate_event");
        result.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task ValidateAsync_UnmappedOrg_ShouldFail422()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "unknown-org" });
        var context = CreateContext("sale.created", "key-1", "sig", payload);
        var body = JsonDocument.Parse(payload).RootElement;

        _secretService.Setup(s => s.FindMatchingSecretAsync("unknown-org", "sig", It.IsAny<string>()))
            .ReturnsAsync("the-secret");
        _idempotencyService.Setup(i => i.IsProcessedAsync("key-1"))
            .ReturnsAsync(false);
        _identityService.Setup(i => i.IsOrganizationReconciledAsync("unknown-org"))
            .ReturnsAsync(false);

        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("unmapped_organization");
        result.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task ValidateAsync_AllValid_ShouldSucceed()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });
        var context = CreateContext("sale.created", "key-1", "sig", payload);
        var body = JsonDocument.Parse(payload).RootElement;

        _secretService.Setup(s => s.FindMatchingSecretAsync("org-123", "sig", It.IsAny<string>()))
            .ReturnsAsync("the-secret");
        _idempotencyService.Setup(i => i.IsProcessedAsync("key-1"))
            .ReturnsAsync(false);
        _identityService.Setup(i => i.IsOrganizationReconciledAsync("org-123"))
            .ReturnsAsync(true);

        var result = await _sut.ValidateAsync(context.Request, body);

        result.Success.Should().BeTrue();
        result.Event.Should().Be("sale.created");
        result.OrganizationId.Should().Be("org-123");
        result.IdempotencyKey.Should().Be("key-1");
    }

    [Fact]
    public async Task ValidateAsync_AllowedEvents_ShouldIncludeFleetEvents()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });

        foreach (var eventName in new[] { "vehicle.created", "trip.created", "fuel_log.created", "machinery.created", "maintenance_log.created" })
        {
            var context = CreateContext(eventName, "key-" + eventName, "sig", payload);
            var body = JsonDocument.Parse(payload).RootElement;

            _secretService.Setup(s => s.FindMatchingSecretAsync("org-123", "sig", It.IsAny<string>()))
                .ReturnsAsync("the-secret");
            _idempotencyService.Setup(i => i.IsProcessedAsync("key-" + eventName))
                .ReturnsAsync(false);
            _identityService.Setup(i => i.IsOrganizationReconciledAsync("org-123"))
                .ReturnsAsync(true);

            var result = await _sut.ValidateAsync(context.Request, body);
            result.Success.Should().BeTrue($"event '{eventName}' should be allowed");
        }
    }

    private static DefaultHttpContext CreateContext(string eventName, string idempotencyKey, string signature, string body)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Method = "POST";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.Headers["X-Webhook-Event"] = eventName;
        context.Request.Headers["X-Idempotency-Key"] = idempotencyKey;
        context.Request.Headers["X-Webhook-Signature"] = signature;
        return context;
    }
}
