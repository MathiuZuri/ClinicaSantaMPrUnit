using Clinica.API.Services;
using Microsoft.AspNetCore.Mvc;
using Clinica.API.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    [Authorize(Policy = PermisosPolicies.AuditoriaVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _auditoriaService.ObtenerTodosAsync());
    }

    [Authorize(Policy = PermisosPolicies.AuditoriaVer)]
    [HttpGet("usuario/{usuarioId:guid}")]
    public async Task<IActionResult> GetByUsuario(Guid usuarioId)
    {
        return Ok(await _auditoriaService.ObtenerPorUsuarioAsync(usuarioId));
    }
}