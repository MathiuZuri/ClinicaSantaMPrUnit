using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.Domain.Interfaces;
using Clinica.Domain.PDFsDto;
using Clinica.Domain.PDFsDto.Interfacespdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers.pdfControladores;

[ApiController]
[Route("api/[controller]")]
[Tags("Documentos PDF - Reportes Financieros")]
public class ReportesFinancierosController : ControllerBase
{
    private readonly IReporteFinancieroPdfService _pdfService;
    private readonly IPagoRepository _pagoRepository;

    public ReportesFinancierosController(IReporteFinancieroPdfService pdfService, IPagoRepository pagoRepository)
    {
        _pdfService = pdfService;
        _pagoRepository = pagoRepository;
    }

    /// <summary>
    /// Genera y descarga el reporte financiero diario en PDF.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite obtener un resumen de los ingresos del día, desglosado por método de pago,
    /// con el detalle de cada movimiento. Útil para el cierre de caja y conciliación contable.
    /// **Permiso requerido:** <see cref="PermisosPolicies.FinanzasVer"/>.
    /// </remarks>
    [HttpGet("diario")]
    [Authorize(Policy = PermisosPolicies.FinanzasVer)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DescargarReporteDiario([FromQuery] DateOnly fecha)
    {
        var pagos = await _pagoRepository.ObtenerTodosConDetalleAsync();
        var pagosDia = pagos
            .Where(p => DateOnly.FromDateTime(p.FechaPago) == fecha && p.Estado != Domain.Enums.EstadoPago.Anulado)
            .ToList();

        var dto = new ReporteDiarioDto
        {
            Fecha = fecha,
            CantidadPagos = pagosDia.Count,
            TotalEfectivo = pagosDia.Where(p => p.MetodoPago == Domain.Enums.MetodoPago.Efectivo).Sum(p => p.MontoPagado),
            TotalYape = pagosDia.Where(p => p.MetodoPago == Domain.Enums.MetodoPago.Yape).Sum(p => p.MontoPagado),
            TotalPlin = pagosDia.Where(p => p.MetodoPago == Domain.Enums.MetodoPago.Plin).Sum(p => p.MontoPagado),
            TotalTransferencia = pagosDia.Where(p => p.MetodoPago == Domain.Enums.MetodoPago.Transferencia).Sum(p => p.MontoPagado),
            TotalTarjeta = pagosDia.Where(p => p.MetodoPago == Domain.Enums.MetodoPago.Tarjeta).Sum(p => p.MontoPagado),
            TotalOtro = pagosDia.Where(p => p.MetodoPago == Domain.Enums.MetodoPago.Otro).Sum(p => p.MontoPagado),
            TotalIngresos = pagosDia.Sum(p => p.MontoPagado),
            Movimientos = pagosDia.Select(p => new MovimientoReporteDto
            {
                CodigoPago = p.CodigoPago,
                Paciente = $"{p.Paciente?.Nombres} {p.Paciente?.Apellidos}",
                Servicio = p.ServicioClinico?.Nombre ?? "",
                Monto = p.MontoPagado,
                MetodoPago = p.MetodoPago.ToString(),
                FechaPago = p.FechaPago
            }).ToList()
        };

        var pdfBytes = _pdfService.GeneratePdf(dto);
        return File(pdfBytes, "application/pdf", $"ReporteDiario_{fecha:yyyyMMdd}.pdf");
    }
}