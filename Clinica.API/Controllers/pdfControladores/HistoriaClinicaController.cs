using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Domain.PDFsDto;
using Clinica.Domain.PDFsDto.Interfacespdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers.pdfControladores;

[ApiController]
[Route("api/[controller]")]
[Tags("Documentos PDF - Historia Clínica")]
public class HistoriaClinicaController : ControllerBase
{
    private readonly IHistoriaClinicaPdfService _pdfService;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IAtencionRepository _atencionRepository;
    private readonly IHistorialClinicoRepository _historialRepository;

    public HistoriaClinicaController(
        IHistoriaClinicaPdfService pdfService,
        IPacienteRepository pacienteRepository,
        IAtencionRepository atencionRepository,
        IHistorialClinicoRepository historialRepository)
    {
        _pdfService = pdfService;
        _pacienteRepository = pacienteRepository;
        _atencionRepository = atencionRepository;
        _historialRepository = historialRepository;
    }

    /// <summary>
    /// Genera y descarga la Historia Clínica completa de un paciente en PDF.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite obtener un documento formal con la ficha de identificación,
    /// antecedentes, funciones vitales, examen obstétrico y las últimas atenciones.
    /// Ideal para entregar al paciente o para expedientes médicos.
    /// **Permiso requerido:** <see cref="PermisosPolicies.HistorialImprimir"/>.
    /// </remarks>
    [HttpGet("paciente/{pacienteId:guid}")]
    [Authorize(Policy = PermisosPolicies.HistorialImprimir)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarHistoriaClinica(Guid pacienteId)
    {
        var paciente = await _pacienteRepository.ObtenerConHistorialAsync(pacienteId);
        if (paciente == null)
            return NotFound(ApiResponse<object>.Error("Paciente no encontrado", 404));

        var historial = paciente.HistorialClinico;
        if (historial == null)
            return NotFound(ApiResponse<object>.Error("El paciente no tiene historial clínico", 404));

        var atenciones = await _atencionRepository.ObtenerPorPacienteAsync(pacienteId);
        var atencionesOrdenadas = atenciones.OrderByDescending(a => a.FechaInicio).ToList();
        var ultimaAtencion = atencionesOrdenadas.FirstOrDefault();

        var anamnesis = ultimaAtencion?.Anamnesis;
        var examenFisico = ultimaAtencion?.ExamenesFisicos?.OrderByDescending(e => e.FechaHoraExamen).FirstOrDefault();
        var diagnostico = ultimaAtencion?.ImpresionDiagnostica;

        var dto = new HistoriaClinicaPdfDto
        {
            NombresApellidos = $"{paciente.Nombres} {paciente.Apellidos}",
            Dni = paciente.DNI,
            FechaNacimiento = paciente.FechaNacimiento,
            Sexo = paciente.Sexo == "M" ? "MASCULINO" : "FEMENINO",
            LugarNacimiento = paciente.LugarNacimiento ?? "",
            Direccion = paciente.Direccion ?? "",
            Correo = paciente.Correo ?? "",
            Celular = paciente.Celular ?? "",
            Ocupacion = paciente.Ocupacion ?? "",
            MotivoConsulta = anamnesis?.MotivoConsulta ?? "",
            NumeroHistoria = historial.CodigoHistorial,
            FechaRegistro = paciente.FechaRegistro,
            Menarquia = "",
            RitmoCatamenial = "",
            Gesta = anamnesis?.Gestaciones ?? 0,
            Partos = anamnesis?.PartosATermino ?? 0,
            Abortos = anamnesis?.Abortos ?? 0,
            HijosVivos = anamnesis?.HijosVivos ?? 0,
            HijosMuertos = 0,
            FUR = anamnesis?.FechaUltimaRegla,
            FPP = anamnesis?.FechaProbableParto,
            PI = "",
            MetodoAnticonceptivo = "",
            PA = "",
            Pulso = "",
            Temperatura = "",
            Respiracion = "",
            SO2 = "",
            Peso = "",
            Talla = "",
            AlturaUterina = examenFisico?.AlturaUterina?.ToString() ?? "",
            Situacion = examenFisico?.SituacionPosicionPresentacion ?? "",
            Presentacion = examenFisico?.SituacionPosicionPresentacion ?? "",
            LatidosCardiacosFetales = examenFisico?.LatidosCardiacosFetales?.ToString() ?? "",
            Edemas = examenFisico?.Edemas ?? "",
            Indicaciones = diagnostico?.IndicacionesReceta ?? "",
            Atenciones = atencionesOrdenadas.Take(5).Select(a => new AtencionResumenDto
            {
                Fecha = a.FechaInicio,
                Servicio = a.ServicioClinico?.Nombre ?? "",
                Doctor = a.Doctor != null ? $"{a.Doctor.Nombres} {a.Doctor.Apellidos}" : "",
                Diagnostico = a.ImpresionDiagnostica?.DiagnosticoPrincipal ?? ""
            }).ToList()
        };

        byte[] pdfBytes;
        try
        {
            pdfBytes = _pdfService.GeneratePdf(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error($"Error al generar el PDF: {ex.Message}", 500));
        }

        var fileName = $"HistoriaClinica_{paciente.DNI}_{DateTime.Now:yyyyMMddHHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}