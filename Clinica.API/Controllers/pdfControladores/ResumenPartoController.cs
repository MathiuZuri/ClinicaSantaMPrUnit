using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.Domain.Interfaces;
using Clinica.Domain.Interfaces.ATENCIONES;
using Clinica.Domain.PDFsDto;
using Clinica.Domain.PDFsDto.Interfacespdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers.pdfControladores;

[ApiController]
[Route("api/[controller]")]
[Tags("Documentos PDF - Resumen de Parto")]
public class ResumenPartoController : ControllerBase
{
    private readonly IResumenPartoPdfService _pdfService;
    private readonly IAtencionRepository _atencionRepository;
    private readonly ITactoVaginalRepository _tactoVaginalRepository;
    private readonly IExamenFisicoRepository _examenFisicoRepository;

    public ResumenPartoController(
        IResumenPartoPdfService pdfService,
        IAtencionRepository atencionRepository,
        ITactoVaginalRepository tactoVaginalRepository,
        IExamenFisicoRepository examenFisicoRepository)
    {
        _pdfService = pdfService;
        _atencionRepository = atencionRepository;
        _tactoVaginalRepository = tactoVaginalRepository;
        _examenFisicoRepository = examenFisicoRepository;
    }

    /// <summary>
    /// Genera y descarga el resumen de parto en PDF para una atención obstétrica.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite obtener un documento con los datos del parto (condición, controles, partograma, datos del recién nacido).
    /// Útil para el expediente médico, certificados de nacimiento o historial obstétrico.
    /// **Permiso requerido:** <see cref="PermisosPolicies.AtencionVer"/>.
    /// </remarks>
    [HttpGet("atencion/{atencionId:guid}")]
    [Authorize(Policy = PermisosPolicies.AtencionVer)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarResumenParto(Guid atencionId)
    {
        var atencion = await _atencionRepository.ObtenerDetalleCompletoAsync(atencionId);
        if (atencion == null)
            return NotFound(ApiResponse<object>.Error("Atención no encontrada", 404));

        var paciente = atencion.Paciente;
        var anamnesis = atencion.Anamnesis;
        var examenes = atencion.ExamenesFisicos?.OrderByDescending(e => e.FechaHoraExamen).ToList();
        var tactos = atencion.TactosVaginales?.OrderBy(t => t.FechaHora).ToList();

        var dto = new ResumenPartoPdfDto
        {
            PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}",
            Dni = paciente.DNI,
            FechaParto = atencion.FechaInicio.Date,
            HoraParto = TimeOnly.FromDateTime(atencion.FechaInicio),
            CondicionParto = "",
            AtendidoPor = atencion.Doctor != null ? $"{atencion.Doctor.Nombres} {atencion.Doctor.Apellidos}" : "",
            FormaTerminacion = "",
            MedicacionExpulsivo = "",
            Episiotomia = "",
            Desgarros = "",
            Alumbramiento = "",
            ModalidadPlacenta = "",
            PesoPlacenta = "",
            LiquidoAmniotico = "",
            ColorLiquido = "",
            LongitudCordon = "",
            PerdidaSanguinea = "",
            ObservacionesMadre = atencion.ImpresionDiagnostica?.DiagnosticosSecundarios ?? "",
            RnVivoMuerto = "",
            SexoRN = "",
            Apgar1Min = "",
            Apgar5Min = "",
            PesoRN = "",
            TallaRN = "",
            PC = "",
            PT = "",
            ObservacionesRN = "",
            DiagnosticoPostParto = atencion.ImpresionDiagnostica?.DiagnosticoPrincipal ?? "",
            ControlesVitales = examenes?.Select(e => new ControlVitalDto
            {
                Hora = TimeOnly.FromDateTime(e.FechaHoraExamen),
                PA = "",
                Pulso = "",
                Temperatura = "",
                Respiracion = ""
            }).ToList() ?? new(),
            RegistrosPartograma = tactos?.Select(t => new PartogramaRegistroDto
            {
                Hora = t.FechaHora.Hour,
                Dilatacion = t.Dilatacion?.ToString() ?? "",
                AlturaPresentacion = t.AlturaPresentacion ?? "",
                DinamicaUterina = t.MembranasOvulares ?? "",
                FrecuenciaCardiacaFetal = atencion.ExamenesFisicos?.FirstOrDefault()?.LatidosCardiacosFetales?.ToString() ?? "",
                Oxitocina = "",
                Medicamentos = "",
                Pulso = "",
                Temperatura = "",
                Orina = ""
            }).ToList() ?? new()
        };

        var pdfBytes = _pdfService.GeneratePdf(dto);
        return File(pdfBytes, "application/pdf", $"ResumenParto_{paciente.DNI}_{DateTime.Now:yyyyMMddHHmm}.pdf");
    }
}