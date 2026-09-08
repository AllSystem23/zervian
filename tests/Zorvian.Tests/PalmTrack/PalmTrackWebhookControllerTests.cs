using System.Text;
using System.Text.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Zorvian.Application.Messages;
using Zorvian.Core.Entities;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;
using Zorvian.Web.Controllers;
using Zorvian.Web.Services.PalmTrack;
using PalmTrackVR = Zorvian.Web.Services.PalmTrack.ValidationResult;

namespace Zorvian.Tests.PalmTrack;

/// <summary>
/// Tests for PalmTrackWebhookController per plan §13.
/// </summary>
public sealed class PalmTrackWebhookControllerTests : IDisposable
{
    private readonly ZorvianDbContext _db;
    private readonly Mock<IPalmTrackWebhookValidator> _validator = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<ILogger<PalmTrackWebhookController>> _logger = new();
    private readonly PalmTrackWebhookController _sut;

    public PalmTrackWebhookControllerTests()
    {
        var options = new DbContextOptionsBuilder<ZorvianDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantContext>();
        tenantMock.Setup(t => t.TenantId).Returns(new TenantId(Guid.NewGuid()));
        tenantMock.Setup(t => t.IsSuperAdmin).Returns(true);
        _db = new ZorvianDbContext(options, tenantMock.Object);

        _sut = new PalmTrackWebhookController(
            _validator.Object,
            _publishEndpoint.Object,
            _db,
            _logger.Object);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task ReceiveWebhook_ValidEvent_ShouldReturn200AndPublish()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123", data = new { } });
        _validator.Setup(v => v.ValidateAsync(It.IsAny<HttpRequest>(), It.IsAny<JsonElement>()))
            .ReturnsAsync(PalmTrackVR.SuccessResult("sale.created", "org-123", "key-1"));
        var ctx = CreateHttpContext(payload, "sale.created", "key-1", "valid-sig");

        var httpContext = CreateHttpContext(payload, "sale.created", "key-1", "valid-sig");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _sut.ReceiveWebhook(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _publishEndpoint.Verify(p => p.Publish(It.IsAny<PalmTrackWebhookReceived>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReceiveWebhook_InvalidSignature_ShouldReturn401()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });
        _validator.Setup(v => v.ValidateAsync(It.IsAny<HttpRequest>(), It.IsAny<JsonElement>()))
            .ReturnsAsync(PalmTrackVR.Fail("invalid_signature", 401));
        var httpContext = CreateHttpContext(payload, "sale.created", "key-1", "bad-sig");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _sut.ReceiveWebhook(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        _publishEndpoint.Verify(p => p.Publish(It.IsAny<PalmTrackWebhookReceived>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReceiveWebhook_EventNotAllowed_ShouldReturn404()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });
        _validator.Setup(v => v.ValidateAsync(It.IsAny<HttpRequest>(), It.IsAny<JsonElement>()))
            .ReturnsAsync(PalmTrackVR.Fail("event_not_allowed", 404, eventName: "bad.event"));
        var httpContext = CreateHttpContext(payload, "bad.event", "key-1", "sig");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _sut.ReceiveWebhook(CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ReceiveWebhook_DuplicateEvent_ShouldReturn409()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });
        _validator.Setup(v => v.ValidateAsync(It.IsAny<HttpRequest>(), It.IsAny<JsonElement>()))
            .ReturnsAsync(PalmTrackVR.Fail("duplicate_event", 409, idempotencyKey: "dup-key"));
        var httpContext = CreateHttpContext(payload, "sale.created", "dup-key", "sig");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _sut.ReceiveWebhook(CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task ReceiveWebhook_UnmappedOrg_ShouldReturn422()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "unknown-org" });
        _validator.Setup(v => v.ValidateAsync(It.IsAny<HttpRequest>(), It.IsAny<JsonElement>()))
            .ReturnsAsync(PalmTrackVR.Fail("unmapped_organization", 422, organizationId: "unknown-org"));
        var httpContext = CreateHttpContext(payload, "sale.created", "key-1", "sig");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _sut.ReceiveWebhook(CancellationToken.None);

        result.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Fact]
    public async Task ReceiveWebhook_MissingIdempotencyKey_ShouldReturn400()
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123" });
        _validator.Setup(v => v.ValidateAsync(It.IsAny<HttpRequest>(), It.IsAny<JsonElement>()))
            .ReturnsAsync(PalmTrackVR.Fail("missing_x_idempotency_key", 400));
        var httpContext = CreateHttpContext(payload, "sale.created", "", "sig");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _sut.ReceiveWebhook(CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ReceiveWebhook_InvalidJson_ShouldReturn500()
    {
        var httpContext = CreateHttpContext("not-json", "sale.created", "key-1", "sig");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _sut.ReceiveWebhook(CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetDlqEvents_ShouldReturnDlqEntries()
    {
        _db.Set<PalmTrackWebhookDlq>().Add(new PalmTrackWebhookDlq
        {
            IdempotencyKey = "test-key",
            Event = "sale.created",
            OrganizationId = "org-123",
            Error = "test error",
            FailedAt = DateTime.UtcNow,
            RetryCount = 3,
            IsResolved = false,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetDlqEvents(limit: 10, unresolvedOnly: true);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        var entries = JsonSerializer.Deserialize<List<JsonElement>>(json);
        entries.Should().HaveCount(1);
    }

    [Fact]
    public async Task Health_ShouldReturn200()
    {
        var result = _sut.Health();
        result.Should().BeOfType<OkObjectResult>();
    }

    private static DefaultHttpContext CreateHttpContext(string body, string eventName, string idempotencyKey, string signature)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Method = "POST";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.Headers["X-Webhook-Event"] = eventName;
        context.Request.Headers["X-Idempotency-Key"] = idempotencyKey;
        context.Request.Headers["X-Webhook-Signature"] = signature;
        // Required for EnableBuffering
        context.RequestServices = new ServiceCollection().BuildServiceProvider();
        return context;
    }
}
