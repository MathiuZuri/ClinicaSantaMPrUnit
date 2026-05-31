using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Horarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HorariosController : ControllerBase
{
    private readonly IHorarioDoctorService _horarioService;

    public HorariosController(IHorarioDoctorService horarioService)
    {
        _horarioService = horarioService;
    }

    [Authorize(Policy = PermisosPolicies.HorarioVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var horarios = await _horarioService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(horarios, "Horarios obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.HorarioVer)]
    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
    {
        var horarios = await _horarioService.ObtenerPorDoctorAsync(doctorId);
        return Ok(ApiResponse<object>.Ok(horarios, "Horarios del doctor obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.HorarioCrear)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearHorarioDoctorDto dto)
    {
        var id = await _horarioService.CrearAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Horario registrado correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.HorarioEditar)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarHorarioDoctorDto dto)
    {
        await _horarioService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Horario actualizado correctamente."));
    }
    
    [Authorize(Policy = PermisosPolicies.HorarioVer)]
    [HttpGet("doctor/{doctorId:guid}/matriz")]
    public async Task<IActionResult> GetMatrizSemanal(Guid doctorId, [FromQuery] string? fecha)
    {
        // Si no se envía fecha, tomamos el día de hoy por defecto
        var fechaFiltro = string.IsNullOrEmpty(fecha) 
            ? DateOnly.FromDateTime(DateTime.Today) 
            : DateOnly.Parse(fecha);

        var matriz = await _horarioService.ObtenerMatrizSemanalAsync(doctorId, fechaFiltro);
        return Ok(ApiResponse<object>.Ok(matriz, "Matriz semanal calculada correctamente."));
    }
}