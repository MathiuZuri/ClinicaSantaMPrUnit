using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Citas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly ICitaService _citaService;

    public CitasController(ICitaService citaService)
    {
        _citaService = citaService;
    }

    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var citas = await _citaService.ObtenerTodasAsync();
        return Ok(ApiResponse<object>.Ok(citas, "Citas obtenidas correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cita = await _citaService.ObtenerPorIdAsync(id);

        if (cita == null)
            throw new KeyNotFoundException("Cita no encontrada.");

        return Ok(ApiResponse<object>.Ok(cita, "Cita obtenida correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var citas = await _citaService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(citas, "Citas del paciente obtenidas correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.CitaVer)]
    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
    {
        var citas = await _citaService.ObtenerPorDoctorAsync(doctorId);
        return Ok(ApiResponse<object>.Ok(citas, "Citas del doctor obtenidas correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.CitaProgramar)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearCitaDto dto)
    {
        var id = await _citaService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Cita programada correctamente.", 201)
        );
    }

    [Authorize(Policy = PermisosPolicies.CitaReprogramar)]
    [HttpPut("{id:guid}/reprogramar")]
    public async Task<IActionResult> Reprogramar(Guid id, [FromBody] ReprogramarCitaDto dto)
    {
        await _citaService.ReprogramarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Cita reprogramada correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.CitaCancelar)]
    [HttpPut("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarCitaDto dto)
    {
        await _citaService.CancelarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Cita cancelada correctamente."));
    }
}