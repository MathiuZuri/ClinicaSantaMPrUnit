using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Atenciones;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtencionesController : ControllerBase
{
    private readonly IAtencionService _atencionService;

    public AtencionesController(IAtencionService atencionService)
    {
        _atencionService = atencionService;
    }

    [Authorize(Policy = PermisosPolicies.AtencionVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var atenciones = await _atencionService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(atenciones, "Atenciones del paciente obtenidas correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.AtencionVer)]
    [HttpGet("cita/{citaId:guid}")]
    public async Task<IActionResult> GetByCita(Guid citaId)
    {
        var atencion = await _atencionService.ObtenerPorCitaAsync(citaId);

        if (atencion == null)
            throw new KeyNotFoundException("Atención no encontrada.");

        return Ok(ApiResponse<object>.Ok(atencion, "Atención obtenida correctamente."));
    }

    [Auditoria("Atenciones", "Atencion", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.AtencionRegistrar)]
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAtencionDto dto)
    {
        var id = await _atencionService.RegistrarAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Atención registrada correctamente."));
    }

    [Auditoria("Atenciones", "Atencion", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.AtencionCerrar)]
    [HttpPut("{id:guid}/cerrar")]
    public async Task<IActionResult> Cerrar(Guid id, [FromBody] CerrarAtencionDto dto)
    {
        await _atencionService.CerrarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Atención cerrada correctamente."));
    }
}