using Clinica.Domain.DTOs.Comprobantes;
using Clinica.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComprobantesController : ControllerBase
{
    private readonly IComprobanteService _comprobanteService;

    public ComprobantesController(IComprobanteService comprobanteService)
    {
        _comprobanteService = comprobanteService;
    }

    // ==========================================================
    // PREVIEWS
    // ==========================================================

    [HttpGet("preview/boleta-pago/{pagoId:guid}")]
    public async Task<ActionResult<ComprobantePagoPreviewDto>> PreviewBoletaPago(Guid pagoId)
    {
        var resultado = await _comprobanteService.PreviewBoletaPagoAsync(pagoId);
        return Ok(resultado);
    }

    [HttpGet("preview/constancia-cita/{citaId:guid}")]
    public async Task<ActionResult<ComprobanteCitaPreviewDto>> PreviewConstanciaCita(Guid citaId)
    {
        var resultado = await _comprobanteService.PreviewConstanciaCitaAsync(citaId);
        return Ok(resultado);
    }

    [HttpGet("preview/resumen-atencion/{atencionId:guid}")]
    public async Task<ActionResult<ComprobanteAtencionPreviewDto>> PreviewResumenAtencion(Guid atencionId)
    {
        var resultado = await _comprobanteService.PreviewResumenAtencionAsync(atencionId);
        return Ok(resultado);
    }

    [HttpGet("preview/estado-cuenta/paciente/{pacienteId:guid}")]
    public async Task<ActionResult<ComprobanteEstadoCuentaPreviewDto>> PreviewEstadoCuentaPaciente(Guid pacienteId)
    {
        var resultado = await _comprobanteService.PreviewEstadoCuentaPacienteAsync(pacienteId);
        return Ok(resultado);
    }

    // ==========================================================
    // EMISIÓN
    // ==========================================================

    [HttpPost("emitir/boleta-pago")]
    public async Task<ActionResult<object>> EmitirBoletaPago([FromBody] EmitirComprobantePagoDto dto)
    {
        var comprobanteId = await _comprobanteService.EmitirBoletaPagoAsync(dto);

        return Ok(new
        {
            Mensaje = "Boleta de pago emitida correctamente.",
            ComprobanteId = comprobanteId
        });
    }

    [HttpPost("emitir/constancia-cita")]
    public async Task<ActionResult<object>> EmitirConstanciaCita([FromBody] EmitirComprobanteCitaDto dto)
    {
        var comprobanteId = await _comprobanteService.EmitirConstanciaCitaAsync(dto);

        return Ok(new
        {
            Mensaje = "Constancia de cita emitida correctamente.",
            ComprobanteId = comprobanteId
        });
    }

    [HttpPost("emitir/resumen-atencion")]
    public async Task<ActionResult<object>> EmitirResumenAtencion([FromBody] EmitirComprobanteAtencionDto dto)
    {
        var comprobanteId = await _comprobanteService.EmitirResumenAtencionAsync(dto);

        return Ok(new
        {
            Mensaje = "Resumen de atención emitido correctamente.",
            ComprobanteId = comprobanteId
        });
    }

    // ==========================================================
    // PDF
    // ==========================================================

    [HttpGet("{comprobanteId:guid}/pdf/boleta-pago")]
    public async Task<IActionResult> GenerarPdfBoletaPago(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfBoletaPagoAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    [HttpGet("{comprobanteId:guid}/pdf/constancia-cita")]
    public async Task<IActionResult> GenerarPdfConstanciaCita(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfConstanciaCitaAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    [HttpGet("{comprobanteId:guid}/pdf/resumen-atencion")]
    public async Task<IActionResult> GenerarPdfResumenAtencion(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfResumenAtencionAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    [HttpGet("{comprobanteId:guid}/pdf/estado-cuenta")]
    public async Task<IActionResult> GenerarPdfEstadoCuenta(Guid comprobanteId)
    {
        var documento = await _comprobanteService.GenerarPdfEstadoCuentaPacienteAsync(comprobanteId);

        return File(
            documento.Archivo,
            documento.ContentType,
            documento.NombreArchivo
        );
    }

    // ==========================================================
    // CONSULTAS
    // ==========================================================

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ComprobanteDto>> ObtenerPorId(Guid id)
    {
        var resultado = await _comprobanteService.ObtenerPorIdAsync(id);
        return Ok(resultado);
    }

    [HttpGet("paciente/{pacienteId:guid}")]
    public async Task<ActionResult<IEnumerable<ComprobanteDto>>> ObtenerPorPaciente(Guid pacienteId)
    {
        var resultado = await _comprobanteService.ObtenerPorPacienteAsync(pacienteId);
        return Ok(resultado);
    }

    [HttpGet("pago/{pagoId:guid}")]
    public async Task<ActionResult<IEnumerable<ComprobanteDto>>> ObtenerPorPago(Guid pagoId)
    {
        var resultado = await _comprobanteService.ObtenerPorPagoAsync(pagoId);
        return Ok(resultado);
    }

    [HttpGet("atencion/{atencionId:guid}")]
    public async Task<ActionResult<IEnumerable<ComprobanteDto>>> ObtenerPorAtencion(Guid atencionId)
    {
        var resultado = await _comprobanteService.ObtenerPorAtencionAsync(atencionId);
        return Ok(resultado);
    }

    // ==========================================================
    // ANULACIÓN
    // ==========================================================

    [HttpPut("{comprobanteId:guid}/anular")]
    public async Task<ActionResult<object>> AnularComprobante(
        Guid comprobanteId,
        [FromBody] AnularComprobanteRequest request)
    {
        await _comprobanteService.AnularComprobanteAsync(comprobanteId, request.Motivo);

        return Ok(new
        {
            Mensaje = "Comprobante anulado correctamente."
        });
    }
}

public class AnularComprobanteRequest
{
    public string Motivo { get; set; } = string.Empty;
}