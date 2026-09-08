using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Infrastructure.Services.PalmTrack;

namespace Zorvian.Tests.PalmTrack;

/// <summary>
/// Tests for PalmTrackWriteService per plan Paso 5 §2.2.
/// Uses Moq.Protected with ItExpr for HttpMessageHandler mocking.
/// </summary>
public sealed class PalmTrackWriteServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<HttpMessageHandler> _handler = new();
    private readonly PalmTrackWriteService _sut;

    public PalmTrackWriteServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PalmTrack:WriteApiBaseUrl"] = "https://palmtrack.test/api/palm/v1",
                ["PalmTrack:WriteApiKey"] = "test-write-key-123",
            })
            .Build();

        var httpClient = new HttpClient(_handler.Object);
        _httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _sut = new PalmTrackWriteService(
            _httpClientFactory.Object,
            config,
            Mock.Of<ILogger<PalmTrackWriteService>>());
    }

    private void SetupResponse(HttpStatusCode statusCode, string body = "{}")
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(body),
            });
    }

    // ── WriteVehicleAsync tests ──

    [Fact]
    public async Task WriteVehicle_Success_ShouldReturnOk()
    {
        SetupResponse(HttpStatusCode.OK, "{\"success\":true}");

        var result = await _sut.WriteVehicleAsync(
            "palm-doc-123", "org-123", status: "disponible", mileage: 15000);

        result.Success.Should().BeTrue();
        result.IdempotencyKey.Should().Contain("vehicle_sync_palm-doc-123");
    }

    [Fact]
    public async Task WriteVehicle_Conflict_ShouldReturn409()
    {
        SetupResponse(HttpStatusCode.Conflict, "{\"error\":\"conflict\"}");

        var result = await _sut.WriteVehicleAsync("palm-doc-123", "org-123");

        result.Success.Should().BeFalse();
        result.IsConflict.Should().BeTrue();
        result.HttpStatusCode.Should().Be(409);
    }

    [Fact]
    public async Task WriteVehicle_Unauthorized_ShouldReturn401()
    {
        SetupResponse(HttpStatusCode.Unauthorized, "{\"error\":\"unauthorized\"}");

        var result = await _sut.WriteVehicleAsync("palm-doc-123", "org-123");

        result.Success.Should().BeFalse();
        result.HttpStatusCode.Should().Be(401);
    }

    // ── WriteUserAsync tests ──

    [Fact]
    public async Task WriteUser_Success_ShouldReturnOk()
    {
        SetupResponse(HttpStatusCode.OK, "{\"success\":true}");

        var result = await _sut.WriteUserAsync(
            "firebase-uid-123", role: "manager", assignedFarms: new List<string> { "farm-1" });

        result.Success.Should().BeTrue();
        result.IdempotencyKey.Should().Be("users_firebase-uid-123");
    }

    [Fact]
    public async Task WriteUser_Conflict_ShouldReturn409()
    {
        SetupResponse(HttpStatusCode.Conflict, "{\"error\":\"conflict\"}");

        var result = await _sut.WriteUserAsync("uid-123", role: "employee");

        result.IsConflict.Should().BeTrue();
    }

    // ── WriteSettingsAsync tests ──

    [Fact]
    public async Task WriteSettings_Success_ShouldReturnOk()
    {
        SetupResponse(HttpStatusCode.OK, "{\"success\":true}");

        var result = await _sut.WriteSettingsAsync(
            "org-123", timezone: "America/Managua", currency: "NIO");

        result.Success.Should().BeTrue();
        result.IdempotencyKey.Should().Contain("settings_org-123");
    }

    [Fact]
    public async Task WriteSettings_NoFields_ShouldReturn400()
    {
        var result = await _sut.WriteSettingsAsync("org-123");

        result.Success.Should().BeFalse();
        result.HttpStatusCode.Should().Be(400);
        result.Error.Should().Contain("No settings");
    }

    // ── WriteInventoryAdjustmentAsync tests ──

    [Fact]
    public async Task WriteInventory_Success_ShouldReturnOk()
    {
        SetupResponse(HttpStatusCode.OK, "{\"success\":true}");

        var result = await _sut.WriteInventoryAdjustmentAsync(
            "item-123", 50, "Recepción de carga", unitCost: 25.50m);

        result.Success.Should().BeTrue();
        result.IdempotencyKey.Should().Contain("inventory_adjust_item-123");
    }

    [Fact]
    public async Task WriteInventory_Conflict_ShouldReturn409()
    {
        SetupResponse(HttpStatusCode.Conflict, "{\"error\":\"conflict\"}");

        var result = await _sut.WriteInventoryAdjustmentAsync(
            "item-123", -10, "Ajuste negativo");

        result.IsConflict.Should().BeTrue();
    }

    // ── HTTP error handling tests ──

    [Fact]
    public async Task WriteVehicle_HttpException_ShouldReturnError()
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _sut.WriteVehicleAsync("palm-doc-123", "org-123");

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Connection error");
    }

    [Fact]
    public async Task WriteVehicle_Timeout_ShouldReturnError()
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var result = await _sut.WriteVehicleAsync("palm-doc-123", "org-123");

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("timeout");
    }

    // ── Header verification tests ──

    [Fact]
    public async Task WriteVehicle_ShouldSendRequiredHeaders()
    {
        HttpRequestMessage? capturedRequest = null;
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}"),
            });

        await _sut.WriteVehicleAsync("palm-doc-123", "org-123");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Should().Contain(h =>
            h.Key == "X-PalmTrack-Write-API-Key" && h.Value.First() == "test-write-key-123");
        capturedRequest.Headers.Should().Contain(h =>
            h.Key == "X-Idempotency-Key");
        capturedRequest.Headers.Should().Contain(h =>
            h.Key == "X-Zorvian-Schema-Version" && h.Value.First() == "vehicles.v1");
    }
}
