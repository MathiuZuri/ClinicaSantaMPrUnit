using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Clinica.Domain.DTOs.Finanzas;
using Clinica.API.Filters;
using Clinica.Domain.Enums;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanzasController : ControllerBase
{
    private readonly IFinanzasService _finanzasService;

    public FinanzasController(IFinanzasService finanzasService)
    {
        _finanzasService = finanzasService;
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-diario")]
    public async Task<IActionResult> ObtenerResumenDiario([FromQuery] DateOnly fecha)
    {
        var resumen = await _finanzasService.ObtenerResumenDiarioAsync(fecha);
        return Ok(ApiResponse<object>.Ok(resumen, "Resumen diario de finanzas obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-mensual")]
    public async Task<IActionResult> ObtenerResumenMensual([FromQuery] int anio, [FromQuery] int mes)
    {
        var resumen = await _finanzasService.ObtenerResumenMensualAsync(anio, mes);
        return Ok(ApiResponse<object>.Ok(resumen, "Resumen mensual de finanzas obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-anual")]
    public async Task<IActionResult> ObtenerResumenAnual([FromQuery] int anio)
    {
        var resumen = await _finanzasService.ObtenerResumenAnualAsync(anio);
        return Ok(ApiResponse<object>.Ok(resumen, "Resumen anual de finanzas obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pagos-pendientes")]
    public async Task<IActionResult> ObtenerPagosPendientes()
    {
        var pagos = await _finanzasService.ObtenerPagosPendientesAsync();
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos pendientes obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pagos-pagados")]
    public async Task<IActionResult> ObtenerPagosPagados()
    {
        var pagos = await _finanzasService.ObtenerPagosPagadosAsync();
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos pagados obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pagos-parciales")]
    public async Task<IActionResult> ObtenerPagosParciales()
    {
        var pagos = await _finanzasService.ObtenerPagosParcialesAsync();
        return Ok(ApiResponse<object>.Ok(pagos, "Pagos parciales obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pago/codigo/{codigoPago}")]
    public async Task<IActionResult> ObtenerPagoPorCodigo(string codigoPago)
    {
        var pago = await _finanzasService.ObtenerPagoPorCodigoAsync(codigoPago);

        if (pago == null)
            throw new KeyNotFoundException("Pago no encontrado.");

        return Ok(ApiResponse<object>.Ok(pago, "Pago obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("paciente/{pacienteId:guid}/estado-cuenta")]
    public async Task<IActionResult> ObtenerEstadoCuentaPaciente(Guid pacienteId)
    {
        var estadoCuenta = await _finanzasService.ObtenerEstadoCuentaPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(estadoCuenta, "Estado de cuenta del paciente obtenido correctamente."));
    }
    
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("deudas-reales")]
    public async Task<IActionResult> ObtenerDeudasReales()
    {
        var deudas = await _finanzasService.ObtenerDeudasRealesAsync();
        return Ok(ApiResponse<object>.Ok(deudas, "Deudas reales obtenidas correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("paciente/{pacienteId:guid}/deudas-reales")]
    public async Task<IActionResult> ObtenerDeudasRealesPaciente(Guid pacienteId)
    {
        var deudas = await _finanzasService.ObtenerDeudasRealesPacienteAsync(pacienteId);
        return Ok(ApiResponse<object>.Ok(deudas, "Deudas reales del paciente obtenidas correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("atencion/{atencionId:guid}/estado-pago")]
    public async Task<IActionResult> ObtenerEstadoPagoAtencion(Guid atencionId)
    {
        var estadoPago = await _finanzasService.ObtenerEstadoPagoAtencionAsync(atencionId);
        return Ok(ApiResponse<object>.Ok(estadoPago, "Estado de pago de la atención obtenido correctamente."));
    }
    
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("libro-diario")]
    public async Task<IActionResult> ObtenerLibroDiario([FromQuery] DateOnly fecha)
    {
        var resultado = await _finanzasService.ObtenerLibroDiarioAsync(fecha);
        return Ok(ApiResponse<object>.Ok(resultado, "Libro diario obtenido correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("resumen-financiero-mensual-completo")]
    public async Task<IActionResult> ObtenerResumenFinancieroMensualCompleto(
        [FromQuery] int anio,
        [FromQuery] int mes)
    {
        var resultado = await _finanzasService.ObtenerResumenFinancieroMensualCompletoAsync(anio, mes);
        return Ok(ApiResponse<object>.Ok(resultado, "Resumen financiero mensual completo obtenido correctamente."));
    }

    [Auditoria("Finanzas", "Ajuste financiero", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.PagoRegistrar)]
    [HttpPost("ajustes-financieros")]
    public async Task<IActionResult> RegistrarAjusteFinanciero([FromBody] RegistrarAjusteFinancieroDto dto)
    {
        var id = await _finanzasService.RegistrarAjusteFinancieroAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Ajuste financiero registrado correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("ajustes-financieros")]
    public async Task<IActionResult> ObtenerAjustesFinancieros()
    {
        var resultado = await _finanzasService.ObtenerAjustesFinancierosAsync();
        return Ok(ApiResponse<object>.Ok(resultado, "Ajustes financieros obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("atencion/{atencionId:guid}/ajustes-financieros")]
    public async Task<IActionResult> ObtenerAjustesPorAtencion(Guid atencionId)
    {
        var resultado = await _finanzasService.ObtenerAjustesPorAtencionAsync(atencionId);
        return Ok(ApiResponse<object>.Ok(resultado, "Ajustes financieros de la atención obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [HttpGet("pago/{pagoId:guid}/ajustes-financieros")]
    public async Task<IActionResult> ObtenerAjustesPorPago(Guid pagoId)
    {
        var resultado = await _finanzasService.ObtenerAjustesPorPagoAsync(pagoId);
        return Ok(ApiResponse<object>.Ok(resultado, "Ajustes financieros del pago obtenidos correctamente."));
    }
}