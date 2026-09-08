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
/// Tests for PalmTrackIdentityService.
/// </summary>
public sealed class PalmTrackIdentityServiceTests : IDisposable
{
    private readonly ZorvianDbContext _db;
    private readonly PalmTrackIdentityService _sut;

    public PalmTrackIdentityServiceTests()
    {
        var options = new DbContextOptionsBuilder<ZorvianDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantContext>();
        tenantMock.Setup(t => t.TenantId).Returns(new TenantId(Guid.NewGuid()));
        _db = new ZorvianDbContext(options, tenantMock.Object);

        _sut = new PalmTrackIdentityService(_db, Mock.Of<ILogger<PalmTrackIdentityService>>());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task IsOrganizationReconciledAsync_MappingExists_ShouldReturnTrue()
    {
        _db.Set<ExternalIdentityMapping>().Add(new ExternalIdentityMapping
        {
            PalmTrackOrgId = "palm-org-123",
            ZorvianTenantId = Guid.NewGuid().ToString(),
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.IsOrganizationReconciledAsync("palm-org-123");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsOrganizationReconciledAsync_InactiveMapping_ShouldReturnFalse()
    {
        _db.Set<ExternalIdentityMapping>().Add(new ExternalIdentityMapping
        {
            PalmTrackOrgId = "palm-org-123",
            ZorvianTenantId = Guid.NewGuid().ToString(),
            IsActive = false,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.IsOrganizationReconciledAsync("palm-org-123");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsOrganizationReconciledAsync_NoMapping_ShouldReturnFalse()
    {
        var result = await _sut.IsOrganizationReconciledAsync("nonexistent");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTenantIdAsync_ValidMapping_ShouldReturnTenantId()
    {
        var tenantId = Guid.NewGuid();
        _db.Set<ExternalIdentityMapping>().Add(new ExternalIdentityMapping
        {
            PalmTrackOrgId = "palm-org-123",
            ZorvianTenantId = tenantId.ToString(),
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetTenantIdAsync("palm-org-123");

        result.Should().Be(tenantId);
    }

    [Fact]
    public async Task GetTenantIdAsync_InvalidGuidFormat_ShouldReturnNull()
    {
        _db.Set<ExternalIdentityMapping>().Add(new ExternalIdentityMapping
        {
            PalmTrackOrgId = "palm-org-123",
            ZorvianTenantId = "not-a-guid",
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetTenantIdAsync("palm-org-123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTenantIdAsync_NoMapping_ShouldReturnNull()
    {
        var result = await _sut.GetTenantIdAsync("nonexistent");
        result.Should().BeNull();
    }
}
