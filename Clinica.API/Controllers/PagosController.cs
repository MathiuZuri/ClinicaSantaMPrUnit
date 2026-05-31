using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Pagos;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    [Authorize(Policy = PermisosPolicies.PagoVer)]
    [HttpGet("paciente/{pacienteId:guid}")]
    public async Task<IActionResult> GetByPaciente(Guid pacienteId)
    {
        var pagos = await _pagoService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos del paciente obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.PagoVer)]
    [HttpGet("cita/{citaId:guid}")]
    public async Task<IActionResult> GetByCita(Guid citaId)
    {
        var pagos = await _pagoService.ObtenerPorCitaAsync(citaId);
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos de la cita obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.PagoVer)]
    [HttpGet("atencion/{atencionId:guid}")]
    public async Task<IActionResult> GetByAtencion(Guid atencionId)
    {
        var pagos = await _pagoService.ObtenerPorAtencionAsync(atencionId);
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos de la atención obtenidos correctamente."));
    }

    [Auditoria("Pagos", "Pago", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.PagoRegistrar)]
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarPagoDto dto)
    {
        var id = await _pagoService.RegistrarAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Pago registrado correctamente."));
    }
    
    [Auditoria("Pagos", "Pago", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.PagoRegistrar)] // o un permiso específico para editar estado
    [HttpPut("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoPagoDto dto)
    {
        await _pagoService.CambiarEstadoAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Estado del pago actualizado correctamente."));
    }
}