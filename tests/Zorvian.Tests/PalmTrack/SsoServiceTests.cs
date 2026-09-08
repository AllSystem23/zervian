using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Zorvian.Application.DTOs.Auth;
using Zorvian.Application.Interfaces;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Core.Entities;
using Zorvian.Core.Enums;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;
using Zorvian.Infrastructure.Services;

namespace Zorvian.Tests.PalmTrack;

/// <summary>
/// Tests for SsoService per plan Paso 6 §11.
/// </summary>
public sealed class SsoServiceTests : IDisposable
{
    private readonly ZorvianDbContext _db;
    private readonly Mock<IFirebaseAuthService> _firebase = new();
    private readonly Mock<IAuthRepository> _authRepo = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<IPalmTrackIdentityService> _identityService = new();
    private readonly Mock<ILogger<SsoService>> _logger = new();
    private readonly SsoService _sut;

    public SsoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ZorvianDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantContext>();
        tenantMock.Setup(t => t.TenantId).Returns(new TenantId(Guid.NewGuid()));
        _db = new ZorvianDbContext(options, tenantMock.Object);

        _sut = new SsoService(
            _firebase.Object,
            _authRepo.Object,
            _jwt.Object,
            _identityService.Object,
            _db,
            _logger.Object);
    }

    public void Dispose() => _db.Dispose();

    // ── SsoLoginAsync tests ──

    [Fact]
    public async Task SsoLogin_InvalidToken_ShouldReturnFailed()
    {
        _firebase.Setup(f => f.VerifyIdTokenAsync("bad-token"))
            .ReturnsAsync((FirebaseUser?)null);

        var result = await _sut.SsoLoginAsync("bad-token", "org-123");

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("inválido");
    }

    [Fact]
    public async Task SsoLogin_ExistingUser_ShouldReturnOk()
    {
        var tenantGuid = Guid.NewGuid();
        var fbUser = new FirebaseUser("uid-123", "test@example.com", "Test User", null);
        _firebase.Setup(f => f.VerifyIdTokenAsync("valid-token"))
            .ReturnsAsync(fbUser);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = "uid-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            TenantId = tenantGuid.ToString(),
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new() { Role = new Role { Name = RoleType.Employee, DisplayName = "Empleado" } }
            }
        };

        _authRepo.Setup(r => r.GetUserByFirebaseUidAsync("uid-123"))
            .ReturnsAsync(existingUser);

        _identityService.Setup(s => s.GetTenantIdAsync("org-123"))
            .ReturnsAsync(tenantGuid);

        _jwt.Setup(j => j.GenerateTokens(It.IsAny<User>(), It.IsAny<Role>(), It.IsAny<string>()))
            .Returns(("access-token", "refresh-token", 3600));

        _authRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.SsoLoginAsync("valid-token", "org-123");

        result.Success.Should().BeTrue();
        result.RequiresProfileCompletion.Should().BeFalse();
        result.AuthResponse.Should().NotBeNull();
        result.AuthResponse!.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task SsoLogin_ExistingUser_Deactivated_ShouldReturnFailed()
    {
        var fbUser = new FirebaseUser("uid-456", "inactive@example.com", "Inactive", null);
        _firebase.Setup(f => f.VerifyIdTokenAsync("valid-token"))
            .ReturnsAsync(fbUser);

        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = "uid-456",
            Email = "inactive@example.com",
            TenantId = "tenant-123",
            IsActive = false,
        };

        _authRepo.Setup(r => r.GetUserByFirebaseUidAsync("uid-456"))
            .ReturnsAsync(inactiveUser);

        var result = await _sut.SsoLoginAsync("valid-token", "org-123");

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("desactivado");
    }

    [Fact]
    public async Task SsoLogin_ExistingUser_TenantMismatch_ShouldReturnFailed()
    {
        var fbUser = new FirebaseUser("uid-789", "user@example.com", "User", null);
        _firebase.Setup(f => f.VerifyIdTokenAsync("valid-token"))
            .ReturnsAsync(fbUser);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = "uid-789",
            Email = "user@example.com",
            TenantId = Guid.NewGuid().ToString(),
            IsActive = true,
            UserRoles = new List<UserRole>()
        };

        _authRepo.Setup(r => r.GetUserByFirebaseUidAsync("uid-789"))
            .ReturnsAsync(user);

        _identityService.Setup(s => s.GetTenantIdAsync("org-123"))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _sut.SsoLoginAsync("valid-token", "org-123");

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("organización");
    }

    [Fact]
    public async Task SsoLogin_NewUser_UnreconciledOrg_ShouldReturnFailed()
    {
        var fbUser = new FirebaseUser("uid-new", "new@example.com", "New User", null);
        _firebase.Setup(f => f.VerifyIdTokenAsync("valid-token"))
            .ReturnsAsync(fbUser);

        _authRepo.Setup(r => r.GetUserByFirebaseUidAsync("uid-new"))
            .ReturnsAsync((User?)null);

        _identityService.Setup(s => s.GetTenantIdAsync("unknown-org"))
            .ReturnsAsync((Guid?)null);

        var result = await _sut.SsoLoginAsync("valid-token", "unknown-org");

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("reconciliada");
    }

    [Fact]
    public async Task SsoLogin_NewUser_ReconciledOrg_ShouldReturnProfileRequired()
    {
        var fbUser = new FirebaseUser("uid-new", "new@example.com", "New User", "pic.jpg");
        _firebase.Setup(f => f.VerifyIdTokenAsync("valid-token"))
            .ReturnsAsync(fbUser);

        _authRepo.Setup(r => r.GetUserByFirebaseUidAsync("uid-new"))
            .ReturnsAsync((User?)null);

        var tenantId = Guid.NewGuid();
        _identityService.Setup(s => s.GetTenantIdAsync("org-123"))
            .ReturnsAsync(tenantId);

        _authRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
            .Callback<User>(u =>
            {
                u.Id = Guid.NewGuid();
                u.TenantId = tenantId.ToString();
            })
            .Returns(Task.CompletedTask);

        _authRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        _jwt.Setup(j => j.GenerateTokens(It.IsAny<User>(), It.IsAny<Role>(), It.IsAny<string>()))
            .Returns(("sso-token", "sso-refresh", 3600));

        // Seed a role in the DB for GetRoleAsync
        _db.Set<Role>().Add(new Role { Name = RoleType.Employee, DisplayName = "Empleado" });
        await _db.SaveChangesAsync();

        var result = await _sut.SsoLoginAsync("valid-token", "org-123", "producer");

        result.Success.Should().BeTrue();
        result.RequiresProfileCompletion.Should().BeTrue();
        result.AuthResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task SsoLogin_WithPalmTrackRole_ShouldMapToZorvianRole()
    {
        var fbUser = new FirebaseUser("uid-mgr", "mgr@example.com", "Manager", null);
        _firebase.Setup(f => f.VerifyIdTokenAsync("valid-token"))
            .ReturnsAsync(fbUser);

        _authRepo.Setup(r => r.GetUserByFirebaseUidAsync("uid-mgr"))
            .ReturnsAsync((User?)null);

        var tenantId = Guid.NewGuid();
        _identityService.Setup(s => s.GetTenantIdAsync("org-123"))
            .ReturnsAsync(tenantId);

        _authRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _authRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        _jwt.Setup(j => j.GenerateTokens(It.IsAny<User>(), It.IsAny<Role>(), It.IsAny<string>()))
            .Returns(("token", "refresh", 3600));

        // Seed Supervisor role
        _db.Set<Role>().Add(new Role { Name = RoleType.Supervisor, DisplayName = "Supervisor" });
        await _db.SaveChangesAsync();

        var result = await _sut.SsoLoginAsync("valid-token", "org-123", "manager");

        result.Success.Should().BeTrue();
        result.RequiresProfileCompletion.Should().BeTrue();
    }

    [Fact]
    public async Task SsoLogin_UnknownRole_ShouldDefaultToEmployee()
    {
        var fbUser = new FirebaseUser("uid-unknown", "unknown@example.com", "Unknown", null);
        _firebase.Setup(f => f.VerifyIdTokenAsync("valid-token"))
            .ReturnsAsync(fbUser);

        _authRepo.Setup(r => r.GetUserByFirebaseUidAsync("uid-unknown"))
            .ReturnsAsync((User?)null);

        var tenantId = Guid.NewGuid();
        _identityService.Setup(s => s.GetTenantIdAsync("org-123"))
            .ReturnsAsync(tenantId);

        _authRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _authRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        _jwt.Setup(j => j.GenerateTokens(It.IsAny<User>(), It.IsAny<Role>(), It.IsAny<string>()))
            .Returns(("token", "refresh", 3600));

        // Seed Employee role
        _db.Set<Role>().Add(new Role { Name = RoleType.Employee, DisplayName = "Empleado" });
        await _db.SaveChangesAsync();

        var result = await _sut.SsoLoginAsync("valid-token", "org-123", "unknown_role_xyz");

        result.Success.Should().BeTrue();
        result.RequiresProfileCompletion.Should().BeTrue();
    }

    // ── CompleteProfileAsync tests ──

    [Fact]
    public async Task CompleteProfile_ValidUser_ShouldReturnTrue()
    {
        var user = new User { Id = Guid.NewGuid(), IsActive = true };
        _authRepo.Setup(r => r.GetUserWithRolesAsync(user.Id))
            .ReturnsAsync(user);
        _authRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.CompleteProfileAsync(
            user.Id, "P001", "Producción", "Productor", "+505 8888-7777");

        result.Should().BeTrue();
        user.Phone.Should().Be("+505 8888-7777");
    }

    [Fact]
    public async Task CompleteProfile_NonexistentUser_ShouldReturnFalse()
    {
        _authRepo.Setup(r => r.GetUserWithRolesAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.CompleteProfileAsync(
            Guid.NewGuid(), "P001", "Dept", "Position");

        result.Should().BeFalse();
    }
}
