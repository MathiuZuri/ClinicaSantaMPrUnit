using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Auth;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [Auditoria("Seguridad", "Usuario", TipoAccionAuditoria.Login, NivelAuditoria.Critico)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] IniciarSesionDto dto)
    {
        var respuesta = await _authService.IniciarSesionAsync(dto);

        return Ok(ApiResponse<object>.Ok(
            respuesta,
            "Inicio de sesión correcto."
        ));
    }
}