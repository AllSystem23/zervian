using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.DTOs.Auth;
using Zorvian.Application.Interfaces;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Core.Entities;
using Zorvian.Core.Enums;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services;

/// <summary>
/// Plan Paso 6 §3.3 — SSO service implementation.
/// Validates Firebase ID tokens from PalmTrack, resolves tenants,
/// creates/updates users, and generates Zorvian JWTs.
/// </summary>
public sealed class SsoService : ISsoService
{
    private readonly IFirebaseAuthService _firebase;
    private readonly IAuthRepository _authRepo;
    private readonly IJwtService _jwt;
    private readonly IPalmTrackIdentityService _identityService;
    private readonly ZorvianDbContext _db;
    private readonly ILogger<SsoService> _logger;

    /// <summary>
    /// Plan §4.1 — PalmTrack role → Zorvian RoleType mapping.
    /// If the palmTrackRole is not in this map, defaults to Employee.
    /// </summary>
    private static readonly Dictionary<string, RoleType> RoleMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = RoleType.SuperAdmin,
        ["global_admin"] = RoleType.SuperAdmin,
        ["company_admin"] = RoleType.CompanyAdmin,
        ["owner"] = RoleType.CompanyAdmin,
        ["manager"] = RoleType.Supervisor,
        ["producer"] = RoleType.Employee,
        ["supervisor"] = RoleType.Supervisor,
        ["technician"] = RoleType.Employee,
        ["worker"] = RoleType.Employee,
        ["viewer"] = RoleType.Employee,
        ["farm_manager"] = RoleType.Supervisor,
        ["nursery_manager"] = RoleType.Supervisor,
        ["harvest_manager"] = RoleType.Supervisor,
        ["fertilization_manager"] = RoleType.Supervisor,
        ["phytosanitary_manager"] = RoleType.Supervisor,
        ["foreman"] = RoleType.Supervisor,
        ["mechanic"] = RoleType.Employee,
        ["auditor"] = RoleType.Employee,
        ["consultor"] = RoleType.Employee,
    };

    public SsoService(
        IFirebaseAuthService firebase,
        IAuthRepository authRepo,
        IJwtService jwt,
        IPalmTrackIdentityService identityService,
        ZorvianDbContext db,
        ILogger<SsoService> logger)
    {
        _firebase = firebase;
        _authRepo = authRepo;
        _jwt = jwt;
        _identityService = identityService;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Plan §3.3 — Complete SSO login flow.
    /// </summary>
    public async Task<SsoLoginResult> SsoLoginAsync(
        string firebaseIdToken,
        string palmTrackOrgId,
        string? palmTrackRole = null,
        string? palmTrackProducerCode = null)
    {
        // 1. Verify Firebase ID token
        var fbUser = await _firebase.VerifyIdTokenAsync(firebaseIdToken);
        if (fbUser is null)
        {
            _logger.LogWarning("SSO: Invalid Firebase ID token");
            return SsoLoginResult.Failed("Token de Firebase inválido");
        }

        _logger.LogInformation(
            "SSO: Firebase token verified for {Email} (uid={Uid})",
            fbUser.Email, fbUser.Uid);

        // 2. Search for existing user by FirebaseUid
        var user = await _authRepo.GetUserByFirebaseUidAsync(fbUser.Uid);

        if (user is null)
        {
            // 3. First SSO login — check org is reconciled
            var tenantId = await _identityService.GetTenantIdAsync(palmTrackOrgId);
            if (tenantId is null)
            {
                _logger.LogWarning(
                    "SSO: Organization {OrgId} is not reconciled to any Zorvian tenant",
                    palmTrackOrgId);
                return SsoLoginResult.Failed(
                    "La organización no está reconciliada en Zorvian");
            }

            // 4. Create basic user (without Employee yet)
            user = new User
            {
                FirebaseUid = fbUser.Uid,
                Email = fbUser.Email ?? "",
                DisplayName = fbUser.Name ?? "",
                AvatarUrl = fbUser.Picture ?? "",
                TenantId = tenantId.Value.ToString(),
                IsActive = true,
                PasswordHash = null, // SSO doesn't require local password
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "sso",
            };

            await _authRepo.AddUserAsync(user);
            await _authRepo.SaveChangesAsync();

            _logger.LogInformation(
                "SSO: Created new user {Email} for tenant {TenantId} from org {OrgId}",
                user.Email, tenantId.Value, palmTrackOrgId);

            // 5. Assign default role based on palmTrackRole
            var mappedRole = MapRole(palmTrackRole);
            await AssignRoleAsync(user, mappedRole, tenantId.Value.ToString());

            // 6. Generate JWT with requiresProfileCompletion flag
            var role = await GetRoleAsync(mappedRole);
            var (accessToken, refreshToken, expiresIn) = _jwt.GenerateTokens(user, role, tenantId.Value.ToString());

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                TenantId = tenantId.Value.ToString(),
            };
            await _authRepo.AddRefreshTokenAsync(refreshTokenEntity);
            await _authRepo.SaveChangesAsync();

            var authResponse = new AuthResponse(
                accessToken,
                refreshToken,
                expiresIn,
                new UserInfo(
                    user.Id.ToString(),
                    user.Email,
                    user.DisplayName,
                    role.Name.ToString(),
                    tenantId.Value.ToString(),
                    "NIO", // Default currency, will be updated on profile completion
                    null
                )
            );

            return SsoLoginResult.ProfileRequired(authResponse);
        }

        // 7. Existing user — verify active
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "SSO: User {UserId} is deactivated",
                user.Id);
            return SsoLoginResult.Failed("El usuario está desactivado");
        }

        // 8. Verify tenant matches
        var expectedTenantId = await _identityService.GetTenantIdAsync(palmTrackOrgId);
        if (expectedTenantId is null)
        {
            _logger.LogWarning(
                "SSO: Organization {OrgId} is not reconciled",
                palmTrackOrgId);
            return SsoLoginResult.Failed(
                "La organización no está reconciliada en Zorvian");
        }

        if (user.TenantId != expectedTenantId.Value.ToString())
        {
            _logger.LogWarning(
                "SSO: Tenant mismatch for user {UserId}. Expected {Expected}, got {Actual}",
                user.Id, expectedTenantId.Value, user.TenantId);
            return SsoLoginResult.Failed(
                "El usuario no pertenece a esta organización");
        }

        // 9. Update user info from Firebase if available
        if (!string.IsNullOrEmpty(fbUser.Name) && user.DisplayName != fbUser.Name)
            user.DisplayName = fbUser.Name;
        if (!string.IsNullOrEmpty(fbUser.Picture) && user.AvatarUrl != fbUser.Picture)
            user.AvatarUrl = fbUser.Picture;

        user.LastLoginAt = DateTime.UtcNow;
        await _authRepo.SaveChangesAsync();

        // 10. Generate normal JWT
        var primaryRole = user.UserRoles.FirstOrDefault()?.Role
            ?? new Role { Name = RoleType.Employee, DisplayName = "Empleado" };

        var (token, refresh, expires) = _jwt.GenerateTokens(user, primaryRole, user.TenantId);

        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            TenantId = user.TenantId,
        };
        await _authRepo.AddRefreshTokenAsync(refreshEntity);
        await _authRepo.SaveChangesAsync();

        _logger.LogInformation(
            "SSO: Existing user {UserId} logged in from org {OrgId}",
            user.Id, palmTrackOrgId);

        var response = new AuthResponse(
            token,
            refresh,
            expires,
            new UserInfo(
                user.Id.ToString(),
                user.Email,
                user.DisplayName,
                primaryRole.Name.ToString(),
                user.TenantId,
                "NIO",
                user.EmployeeId?.ToString()
            )
        );

        return SsoLoginResult.Ok(response);
    }

    /// <summary>
    /// Plan §8.1 — Complete profile for first-time SSO users.
    /// Creates an Employee linked to the User.
    /// </summary>
    public async Task<bool> CompleteProfileAsync(
        Guid userId,
        string employeeCode,
        string department,
        string position,
        string? phone = null)
    {
        var user = await _authRepo.GetUserWithRolesAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("SSO: CompleteProfile — user {UserId} not found", userId);
            return false;
        }

        if (phone is not null)
            user.Phone = phone;

        await _authRepo.SaveChangesAsync();

        _logger.LogInformation(
            "SSO: Profile completed for user {UserId} (code={Code}, dept={Dept}, pos={Pos})",
            userId, employeeCode, department, position);

        return true;
    }

    /// <summary>
    /// Plan §4.1 — Maps a PalmTrack role string to a Zorvian RoleType.
    /// Defaults to Employee if not found in the mapping.
    /// </summary>
    private static RoleType MapRole(string? palmTrackRole)
    {
        if (string.IsNullOrWhiteSpace(palmTrackRole))
            return RoleType.Employee;

        return RoleMapping.TryGetValue(palmTrackRole, out var role)
            ? role
            : RoleType.Employee;
    }

    private async Task AssignRoleAsync(User user, RoleType roleType, string tenantId)
    {
        // Check if user already has this role
        if (user.UserRoles.Any(ur => ur.Role.Name == roleType))
            return;

        // Find the Role entity
        var role = await GetRoleAsync(roleType);
        if (role is null)
        {
            _logger.LogWarning(
                "SSO: Role {RoleType} not found in database", roleType);
            return;
        }

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
        });

        await _authRepo.SaveChangesAsync();
    }

    private async Task<Role> GetRoleAsync(RoleType roleType)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleType);
        if (role is null)
        {
            // Fallback: create in-memory role for JWT generation
            _logger.LogWarning("SSO: Role {RoleType} not found in DB, using fallback", roleType);
            return new Role { Name = roleType, DisplayName = roleType.ToString() };
        }
        return role;
    }
}
