using Zorvian.Application.DTOs.Auth;

namespace Zorvian.Application.Interfaces;

/// <summary>
/// Plan Paso 6 §9.1 — SSO service for shared authentication between PalmTrack and Zorvian.
/// </summary>
public interface ISsoService
{
    /// <summary>
    /// Validates a Firebase ID token from PalmTrack, resolves the tenant,
    /// and generates a Zorvian JWT. Creates the user if first SSO login.
    /// </summary>
    /// <param name="firebaseIdToken">Firebase ID token from PalmTrack redirect.</param>
    /// <param name="palmTrackOrgId">Organization ID from PalmTrack claims/query.</param>
    /// <param name="palmTrackRole">Role from PalmTrack custom claims (optional).</param>
    /// <param name="palmTrackProducerCode">Producer code from PalmTrack custom claims (optional).</param>
    /// <returns>Auth response with JWT, or null if SSO validation fails.</returns>
    Task<SsoLoginResult> SsoLoginAsync(
        string firebaseIdToken,
        string palmTrackOrgId,
        string? palmTrackRole = null,
        string? palmTrackProducerCode = null);

    /// <summary>
    /// Completes the profile for a first-time SSO user.
    /// Creates an Employee linked to the User.
    /// </summary>
    Task<bool> CompleteProfileAsync(
        Guid userId,
        string employeeCode,
        string department,
        string position,
        string? phone = null);
}

/// <summary>
/// Result of an SSO login attempt.
/// </summary>
public sealed class SsoLoginResult
{
    public bool Success { get; init; }
    public bool RequiresProfileCompletion { get; init; }
    public AuthResponse? AuthResponse { get; init; }
    public string? Error { get; init; }

    public static SsoLoginResult Ok(AuthResponse response) => new()
    {
        Success = true,
        AuthResponse = response,
    };

    public static SsoLoginResult ProfileRequired(AuthResponse response) => new()
    {
        Success = true,
        RequiresProfileCompletion = true,
        AuthResponse = response,
    };

    public static SsoLoginResult Failed(string error) => new()
    {
        Success = false,
        Error = error,
    };
}
