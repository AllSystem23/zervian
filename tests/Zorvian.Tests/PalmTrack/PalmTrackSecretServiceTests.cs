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
/// Tests for PalmTrackSecretService.
/// </summary>
public sealed class PalmTrackSecretServiceTests : IDisposable
{
    private readonly ZorvianDbContext _db;
    private readonly PalmTrackSecretService _sut;

    public PalmTrackSecretServiceTests()
    {
        var options = new DbContextOptionsBuilder<ZorvianDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantContext>();
        tenantMock.Setup(t => t.TenantId).Returns(new TenantId(Guid.NewGuid()));
        _db = new ZorvianDbContext(options, tenantMock.Object);

        _sut = new PalmTrackSecretService(_db, Mock.Of<ILogger<PalmTrackSecretService>>());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetActiveSecretAsync_WithActiveSecret_ShouldReturnSecret()
    {
        _db.Set<PalmTrackWebhookSecret>().Add(new PalmTrackWebhookSecret
        {
            OrganizationId = "org-123",
            SecretHash = "my-secret-value",
            SecretPrefix = "my-secre",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetActiveSecretAsync("org-123");

        result.Should().Be("my-secret-value");
    }

    [Fact]
    public async Task GetActiveSecretAsync_WithExpiredSecret_ShouldReturnNull()
    {
        _db.Set<PalmTrackWebhookSecret>().Add(new PalmTrackWebhookSecret
        {
            OrganizationId = "org-123",
            SecretHash = "expired-secret",
            SecretPrefix = "expired-",
            ValidFrom = DateTime.UtcNow.AddDays(-10),
            ValidTo = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetActiveSecretAsync("org-123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSecretAsync_WithInactiveSecret_ShouldReturnNull()
    {
        _db.Set<PalmTrackWebhookSecret>().Add(new PalmTrackWebhookSecret
        {
            OrganizationId = "org-123",
            SecretHash = "inactive-secret",
            SecretPrefix = "inactive",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            IsActive = false,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetActiveSecretAsync("org-123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSecretAsync_WithNoSecrets_ShouldReturnNull()
    {
        var result = await _sut.GetActiveSecretAsync("nonexistent-org");
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindMatchingSecretAsync_CorrectSignature_ShouldReturnSecret()
    {
        var secret = "test-secret-123";
        _db.Set<PalmTrackWebhookSecret>().Add(new PalmTrackWebhookSecret
        {
            OrganizationId = "org-123",
            SecretHash = secret,
            SecretPrefix = "test-sec",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        // Compute correct HMAC
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("test-body"));
        var correctSig = Convert.ToHexString(hash).ToLowerInvariant();

        var result = await _sut.FindMatchingSecretAsync("org-123", correctSig, "test-body");

        result.Should().Be(secret);
    }

    [Fact]
    public async Task FindMatchingSecretAsync_WrongSignature_ShouldReturnNull()
    {
        _db.Set<PalmTrackWebhookSecret>().Add(new PalmTrackWebhookSecret
        {
            OrganizationId = "org-123",
            SecretHash = "test-secret-123",
            SecretPrefix = "test-sec",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.FindMatchingSecretAsync("org-123", "wrong-signature", "test-body");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindMatchingSecretAsync_NoSecretsForOrg_ShouldReturnNull()
    {
        var result = await _sut.FindMatchingSecretAsync("nonexistent-org", "any-sig", "any-body");
        result.Should().BeNull();
    }
}
