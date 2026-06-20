using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Auth;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Autenticación y Seguridad")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inicia sesión de un usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite a un usuario autenticarse utilizando su correo o nombre de usuario y contraseña.
    /// Retorna un token JWT que debe ser incluido en las siguientes peticiones.
    /// **Permiso:** Público (no requiere autenticación previa).
    /// </remarks>
    [AllowAnonymous]
    [Auditoria("Seguridad", "Usuario", TipoAccionAuditoria.Login, NivelAuditoria.Critico)]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<RespuestaInicioSesionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] IniciarSesionDto dto)
    {
        var respuesta = await _authService.IniciarSesionAsync(dto);
        return Ok(ApiResponse<object>.Ok(respuesta, "Inicio de sesión correcto."));
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite al usuario cambiar su propia contraseña. Requiere la contraseña actual para validación.
    /// **Requisito:** Usuario autenticado (cualquier rol).
    /// </remarks>
    [Authorize]
    [Auditoria("Seguridad", "Usuario", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [HttpPost("cambiar-contrasena")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaDto dto)
    {
        // El servicio obtiene el ID del usuario desde el token JWT
        await _authService.CambiarContrasenaAsync(dto);
        return Ok(ApiResponse<object>.Ok(null, "Contraseña actualizada correctamente."));
    }
}