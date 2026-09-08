using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Zorvian.Core.Entities;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;
using Zorvian.Infrastructure.Services.PalmTrack;

namespace Zorvian.Tests.PalmTrack;

/// <summary>
/// Tests for PalmTrackIdempotencyService.
/// </summary>
public sealed class PalmTrackIdempotencyServiceTests : IDisposable
{
    private readonly ZorvianDbContext _db;
    private readonly PalmTrackIdempotencyService _sut;

    public PalmTrackIdempotencyServiceTests()
    {
        var options = new DbContextOptionsBuilder<ZorvianDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantContext>();
        tenantMock.Setup(t => t.TenantId).Returns(new TenantId(Guid.NewGuid()));
        _db = new ZorvianDbContext(options, tenantMock.Object);

        _sut = new PalmTrackIdempotencyService(_db, Mock.Of<ILogger<PalmTrackIdempotencyService>>());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task IsProcessedAsync_ProcessedKey_ShouldReturnTrue()
    {
        _db.Set<PalmTrackWebhookLog>().Add(new PalmTrackWebhookLog
        {
            IdempotencyKey = "processed-key",
            Event = "sale.created",
            OrganizationId = "org-123",
            Status = "processed",
            ReceivedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.IsProcessedAsync("processed-key");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsProcessedAsync_FailedKey_ShouldReturnFalse()
    {
        _db.Set<PalmTrackWebhookLog>().Add(new PalmTrackWebhookLog
        {
            IdempotencyKey = "failed-key",
            Event = "sale.created",
            OrganizationId = "org-123",
            Status = "failed",
            ReceivedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.IsProcessedAsync("failed-key");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProcessedAsync_NonExistentKey_ShouldReturnFalse()
    {
        var result = await _sut.IsProcessedAsync("nonexistent-key");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkProcessedAsync_ShouldCreateLogEntry()
    {
        await _sut.MarkProcessedAsync("new-key", "sale.created", "org-123", "{}");

        var log = await _db.Set<PalmTrackWebhookLog>()
            .FirstOrDefaultAsync(l => l.IdempotencyKey == "new-key");

        log.Should().NotBeNull();
        log!.Event.Should().Be("sale.created");
        log.OrganizationId.Should().Be("org-123");
        log.Status.Should().Be("processed");
        log.Payload.Should().Be("{}");
    }

    [Fact]
    public async Task MarkFailedAsync_ExistingKey_ShouldUpdateStatus()
    {
        _db.Set<PalmTrackWebhookLog>().Add(new PalmTrackWebhookLog
        {
            IdempotencyKey = "fail-key",
            Event = "sale.created",
            OrganizationId = "org-123",
            Status = "processed",
            ReceivedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        await _sut.MarkFailedAsync("fail-key", "test error");

        var log = await _db.Set<PalmTrackWebhookLog>()
            .FirstOrDefaultAsync(l => l.IdempotencyKey == "fail-key");

        log!.Status.Should().Be("failed");
        log.Error.Should().Be("test error");
    }
}
