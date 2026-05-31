using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermisosController : ControllerBase
{
    private readonly IPermisoService _permisoService;

    public PermisosController(IPermisoService permisoService)
    {
        _permisoService = permisoService;
    }

    [Authorize(Policy = PermisosPolicies.PermisoVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permisos = await _permisoService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(permisos, "Permisos obtenidos correctamente."));
    }
}