using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Pacientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;

    public PacientesController(IPacienteService pacienteService)
    {
        _pacienteService = pacienteService;
    }

    [Authorize(Policy = PermisosPolicies.PacienteVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _pacienteService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(pacientes, "Pacientes obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.PacienteVer)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var paciente = await _pacienteService.ObtenerPorIdAsync(id);

        if (paciente == null)
            throw new KeyNotFoundException("Paciente no encontrado.");

        return Ok(ApiResponse<object>.Ok(paciente, "Paciente obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.PacienteVer)]
    [HttpGet("dni/{dni}")]
    public async Task<IActionResult> GetByDni(string dni)
    {
        var paciente = await _pacienteService.ObtenerPorDniAsync(dni);

        if (paciente == null)
            throw new KeyNotFoundException("Paciente no encontrado.");

        return Ok(ApiResponse<object>.Ok(paciente, "Paciente obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.PacienteCrear)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearPacienteDto dto)
    {
        var id = await _pacienteService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Paciente registrado correctamente.", 201)
        );
    }

    [Authorize(Policy = PermisosPolicies.PacienteEditar)]
    [HttpPut("{id:guid}/contacto")]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] ActualizarContactoPacienteDto dto)
    {
        await _pacienteService.ActualizarContactoAsync(id, dto);

        return Ok(ApiResponse<object>.Ok(new { id }, "Contacto del paciente actualizado correctamente."));
    }
    
    [Authorize(Policy = PermisosPolicies.PacienteEditar)]
    [HttpPut("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoPacienteDto dto)
    {
        await _pacienteService.CambiarEstadoAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Estado del paciente actualizado correctamente."));
    }
}