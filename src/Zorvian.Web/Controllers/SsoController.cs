using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zorvian.Application.DTOs.Auth;
using Zorvian.Application.Interfaces;
using Zorvian.Web.Filters;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Zorvian.Web.Controllers;

/// <summary>
/// Plan Paso 6 §9.1 — SSO controller for shared authentication with PalmTrack.
/// </summary>
[ApiController]
[Route("zorvian/v1/auth")]
public sealed class SsoController : ControllerBase
{
    private readonly ISsoService _ssoService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SsoController> _logger;

    public SsoController(ISsoService ssoService, IConfiguration configuration, ILogger<SsoController> logger)
    {
        _ssoService = ssoService;
        _configuration = configuration;
        _logger = logger;
    }

    private bool IsSsoEnabled => _configuration.GetValue<bool>("PalmTrack:SsoEnabled");

    /// <summary>
    /// Plan §3.2 — SSO login endpoint.
    /// PalmTrack redirects here with Firebase ID token and organization ID.
    /// 
    /// GET zorvian/v1/auth/sso/palmtrack?token=...&amp;orgId=...&amp;returnUrl=/dashboard
    /// </summary>
    [HttpGet("sso/palmtrack")]
    [AllowAnonymous]
    [Audit("Auth", "SsoLogin")]
    public async Task<IActionResult> SsoLogin(
        [FromQuery] string token,
        [FromQuery] string orgId,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? palmTrackRole = null,
        [FromQuery] string? palmTrackProducerCode = null)
    {
        // Plan §10.1 — Feature flag: if SSO disabled, return 404
        if (!IsSsoEnabled)
        {
            _logger.LogWarning("SSO: Endpoint disabled by feature flag (PALMTRACK_SSO_ENABLED)");
            return NotFound(new { error = "sso_disabled", message = "SSO is not enabled" });
        }

        // Validate required parameters
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { error = "missing_token", message = "Token parameter is required" });

        if (string.IsNullOrWhiteSpace(orgId))
            return BadRequest(new { error = "missing_org_id", message = "orgId parameter is required" });

        _logger.LogInformation(
            "SSO: Login request for org {OrgId} with returnUrl {ReturnUrl}",
            orgId, returnUrl ?? "/dashboard");

        var result = await _ssoService.SsoLoginAsync(
            token, orgId, palmTrackRole, palmTrackProducerCode);

        if (!result.Success)
        {
            _logger.LogWarning("SSO: Login failed — {Error}", result.Error);
            return Unauthorized(new { error = "sso_failed", message = result.Error });
        }

        // Plan §8.1 — If profile completion required, return with flag
        if (result.RequiresProfileCompletion)
        {
            return Ok(new
            {
                requires_profile_completion = true,
                data = result.AuthResponse,
                redirect = returnUrl ?? "/dashboard",
            });
        }

        // Plan §8.2 — Normal access: return JWT directly
        return Ok(new
        {
            data = result.AuthResponse,
            redirect = returnUrl ?? "/dashboard",
        });
    }

    /// <summary>
    /// Plan §8.1 — Complete profile for first-time SSO users.
    /// Creates an Employee linked to the User.
    /// </summary>
    [HttpPost("complete-profile")]
    [Authorize]
    [Audit("Auth", "CompleteProfile")]
    public async Task<IActionResult> CompleteProfile(
        [FromBody] CompleteProfileRequest request)
    {
        // Get userId from JWT claims
        var userIdClaim = User.FindFirst("user_id")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "invalid_token", message = "User ID not found in token" });

        if (string.IsNullOrWhiteSpace(request.EmployeeCode))
            return BadRequest(new { error = "missing_employee_code", message = "Employee code is required" });

        if (string.IsNullOrWhiteSpace(request.Department))
            return BadRequest(new { error = "missing_department", message = "Department is required" });

        if (string.IsNullOrWhiteSpace(request.Position))
            return BadRequest(new { error = "missing_position", message = "Position is required" });

        var success = await _ssoService.CompleteProfileAsync(
            userId,
            request.EmployeeCode,
            request.Department,
            request.Position,
            request.Phone);

        if (!success)
            return NotFound(new { error = "user_not_found", message = "User not found" });

        return Ok(new { message = "Profile completed successfully" });
    }
}

/// <summary>
/// Request body for profile completion.
/// </summary>
public sealed record CompleteProfileRequest(
    string EmployeeCode,
    string Department,
    string Position,
    string? Phone = null);
