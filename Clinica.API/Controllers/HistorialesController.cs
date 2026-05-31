using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistorialesController : ControllerBase
{
    private readonly IHistorialClinicoService _historialService;

    public HistorialesController(IHistorialClinicoService historialService)
    {
        _historialService = historialService;
    }

    [Authorize(Policy = PermisosPolicies.HistorialVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var historial = await _historialService.ObtenerPorPacienteAsync(pacienteId);

        if (historial == null)
            throw new KeyNotFoundException("Historial clínico no encontrado.");

        return Ok(ApiResponse<object>.Ok(historial, "Historial clínico obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.HistorialVer)]
    [HttpGet("{historialId:guid}/detalles")]
    public async Task<IActionResult> GetConDetalles(Guid historialId)
    {
        var historial = await _historialService.ObtenerConDetallesAsync(historialId);

        if (historial == null)
            throw new KeyNotFoundException("Historial clínico no encontrado.");

        return Ok(ApiResponse<object>.Ok(historial, "Historial clínico con detalles obtenido correctamente."));
    }
}