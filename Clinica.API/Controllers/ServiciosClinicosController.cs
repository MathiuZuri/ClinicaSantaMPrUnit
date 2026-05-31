using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiciosClinicosController : ControllerBase
{
    private readonly IServicioClinicoService _servicioService;

    public ServiciosClinicosController(IServicioClinicoService servicioService)
    {
        _servicioService = servicioService;
    }

    [Authorize(Policy = PermisosPolicies.ServicioVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var servicios = await _servicioService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(servicios, "Servicios clínicos obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.ServicioVer)]
    [HttpGet("activos")]
    public async Task<IActionResult> GetActivos()
    {
        var servicios = await _servicioService.ObtenerActivosAsync();
        return Ok(ApiResponse<object>.Ok(servicios, "Servicios clínicos activos obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.ServicioVer)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var servicio = await _servicioService.ObtenerPorIdAsync(id);

        if (servicio == null)
            throw new KeyNotFoundException("Servicio clínico no encontrado.");

        return Ok(ApiResponse<object>.Ok(servicio, "Servicio clínico obtenido correctamente."));
    }
}