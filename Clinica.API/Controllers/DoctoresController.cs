using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Doctores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctoresController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctoresController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctores = await _doctorService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(doctores, "Doctores obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet("activos")]
    public async Task<IActionResult> GetActivos()
    {
        var doctores = await _doctorService.ObtenerActivosAsync();
        return Ok(ApiResponse<object>.Ok(doctores, "Doctores activos obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var doctor = await _doctorService.ObtenerPorIdAsync(id);

        if (doctor == null)
            throw new KeyNotFoundException("Doctor no encontrado.");

        return Ok(ApiResponse<object>.Ok(doctor, "Doctor obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.DoctorCrear)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearDoctorDto dto)
    {
        var id = await _doctorService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Doctor registrado correctamente.", 201)
        );
    }

    [Authorize(Policy = PermisosPolicies.DoctorEditar)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarDoctorDto dto)
    {
        await _doctorService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Doctor actualizado correctamente."));
    }
}