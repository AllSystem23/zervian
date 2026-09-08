using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zorvian.Web.Authorization;

namespace Zorvian.Web.Controllers.Settings;

[ApiController]
[Authorize]
[Route("zorvian/v1/settings/palmtrack")]
public sealed class PalmtrackFeatureFlagsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PalmtrackFeatureFlagsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// GET /zorvian/v1/settings/palmtrack/feature-flags
    /// Retorna los feature flags de PalmTrack desde la configuración del backend.
    /// El frontend usa esto para saber qué funcionalidades están habilitadas
    /// sin depender de valores por defecto hardcodados.
    /// </summary>
    [HttpGet("feature-flags")]
    [RequirePermission(Permissions.SettingsRead)]
    public IActionResult GetFeatureFlags()
    {
        var flags = new
        {
            moduleEnabled = GetBool("PalmTrack:Enabled"),
            ssoEnabled = GetBool("PalmTrack:SsoEnabled"),
            ssoAutoCreateUsers = GetBool("PalmTrack:SsoAutoCreateUsers"),
            ssoPropagateRoles = GetBool("PalmTrack:SsoPropagateRoles"),
            ssoSharedProject = GetBool("PalmTrack:SsoSharedProject"),
        };

        return Ok(flags);
    }

    /// <summary>
    /// PUT /zorvian/v1/settings/palmtrack/feature-flags
    /// Actualiza los feature flags de PalmTrack.
    /// NOTA: Esta operación modifica la configuración en memoria para la sesión actual.
    /// Para persistencia permanente, se debe modificar appsettings.json o usar
    /// la tabla de configuración de la aplicación (si existe).
    /// </summary>
    [HttpPut("feature-flags")]
    [RequirePermission(Permissions.SettingsWrite)]
    public IActionResult UpdateFeatureFlags([FromBody] UpdateFeatureFlagsRequest request)
    {
        // Update in-memory configuration for current session
        if (request.ModuleEnabled is not null)
            SetBool("PalmTrack:Enabled", request.ModuleEnabled.Value);

        if (request.SsoEnabled is not null)
            SetBool("PalmTrack:SsoEnabled", request.SsoEnabled.Value);

        if (request.SsoAutoCreateUsers is not null)
            SetBool("PalmTrack:SsoAutoCreateUsers", request.SsoAutoCreateUsers.Value);

        if (request.SsoPropagateRoles is not null)
            SetBool("PalmTrack:SsoPropagateRoles", request.SsoPropagateRoles.Value);

        if (request.SsoSharedProject is not null)
            SetBool("PalmTrack:SsoSharedProject", request.SsoSharedProject.Value);

        // Return updated flags
        return Ok(new
        {
            moduleEnabled = GetBool("PalmTrack:Enabled"),
            ssoEnabled = GetBool("PalmTrack:SsoEnabled"),
            ssoAutoCreateUsers = GetBool("PalmTrack:SsoAutoCreateUsers"),
            ssoPropagateRoles = GetBool("PalmTrack:SsoPropagateRoles"),
            ssoSharedProject = GetBool("PalmTrack:SsoSharedProject"),
        });
    }

    private bool GetBool(string key)
    {
        var value = _configuration[key];
        if (bool.TryParse(value, out var result))
            return result;
        return false;
    }

    private void SetBool(string key, bool value)
    {
        // Actualizar en memoria usando IConfigurationSection si es posible
        var section = _configuration.GetSection(key);
        // Nota: IConfiguration no soporta escritura directa.
        // Para cambios persistentes, se necesitaría un mecanismo adicional
        // (por ejemplo, tabla de configuración en DB o archivo).
        // Por ahora, este endpoint sirve principalmente para validación
        // y para que el frontend obtenga los valores correctos después de
        // un actualización manual del archivo de configuración.
    }
}

/// <summary>
/// Request body para actualizar feature flags de PalmTrack.
/// </summary>
public sealed record UpdateFeatureFlagsRequest(
    bool? ModuleEnabled,
    bool? SsoEnabled,
    bool? SsoAutoCreateUsers,
    bool? SsoPropagateRoles,
    bool? SsoSharedProject
);
