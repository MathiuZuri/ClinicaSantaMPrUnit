using Clinica.Domain.DTOs.Atenciones;
using Clinica.Domain.DTOs.Atenciones.Modulos;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class AtencionService : IAtencionService
{
    private readonly IAtencionRepository _atencionRepository;
    private readonly ICitaRepository _citaRepository;
    private readonly IPagoRepository _pagoRepository;
    private readonly IHistorialDetalleRepository _historialDetalleRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IServicioClinicoRepository _servicioRepository;

    public AtencionService(
        IAtencionRepository atencionRepository,
        ICitaRepository citaRepository,
        IPagoRepository pagoRepository,
        IHistorialDetalleRepository historialDetalleRepository,
        IUsuarioActualService usuarioActualService,
        IPacienteRepository pacienteRepository,
        IServicioClinicoRepository servicioRepository)
    {
        _atencionRepository = atencionRepository;
        _citaRepository = citaRepository;
        _pagoRepository = pagoRepository;
        _historialDetalleRepository = historialDetalleRepository;
        _usuarioActualService = usuarioActualService;
        _pacienteRepository = pacienteRepository;
        _servicioRepository = servicioRepository;
    }

    public async Task<IEnumerable<AtencionResponseDto>> ObtenerTodasAsync()
    {
        var atenciones = await _atencionRepository.GetAllAsync();
        return atenciones.Select(MapearAtencion);
    }

    public async Task<IEnumerable<AtencionResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        var atenciones = await _atencionRepository.ObtenerPorPacienteAsync(pacienteId);
        return atenciones.Select(MapearAtencion);
    }

    public async Task<AtencionResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var atencion = await _atencionRepository.ObtenerDetalleCompletoAsync(id);
        if (atencion == null) return null;
        return MapearAtencion(atencion);
    }

    public async Task<Guid> RegistrarAtencionAsync(RegistrarAtencionDto dto)
    {
        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var paciente = await _pacienteRepository.GetByIdAsync(dto.PacienteId)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        var servicio = await _servicioRepository.GetByIdAsync(dto.ServicioClinicoId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        // ✅ SOLUCIÓN AL ERROR: Validamos que envíen el Historial y extraemos el Guid
        if (!dto.HistorialClinicoId.HasValue || dto.HistorialClinicoId.Value == Guid.Empty)
            throw new InvalidOperationException("El identificador del historial clínico es obligatorio.");

        Guid historialIdReal = dto.HistorialClinicoId.Value;

        // 1. Creación del CORE (Atención base)
        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = GenerarCodigoAtencion(servicio.CodigoServicio),
            PacienteId = dto.PacienteId,
            DoctorId = dto.DoctorId,
            ServicioClinicoId = dto.ServicioClinicoId,
            CitaId = dto.CitaId,
            HistorialClinicoId = historialIdReal, // ✅ ERROR CORREGIDO AQUÍ
            FechaInicio = DateTime.UtcNow,
            Estado = EstadoAtencion.Abierta
        };

        // 2. Acoplamiento de MÓDULOS INDEPENDIENTES
        if (dto.Anamnesis != null)
        {
            atencion.Anamnesis = new Anamnesis
            {
                Id = Guid.NewGuid(),
                MotivoConsulta = dto.Anamnesis.MotivoConsulta,
                Gestaciones = dto.Anamnesis.Gestaciones,
                HijosVivos = dto.Anamnesis.HijosVivos,
                Abortos = dto.Anamnesis.Abortos,
                PartosPretermino = dto.Anamnesis.PartosPretermino,
                PartosATermino = dto.Anamnesis.PartosATermino,
                FechaUltimaRegla = dto.Anamnesis.FechaUltimaRegla,
                FechaProbableParto = dto.Anamnesis.FechaProbableParto,
                EdadGestacional = dto.Anamnesis.EdadGestacional,
                Alergias = dto.Anamnesis.Alergias,
                EnfermedadesCronicas = dto.Anamnesis.EnfermedadesCronicas,
                CirugiasPrevias = dto.Anamnesis.CirugiasPrevias,
                AntecedentesAdicionales = dto.Anamnesis.AntecedentesAdicionales
            };
        }

        atencion.ExamenesFisicos = dto.ExamenesFisicos?.Select(e => new ExamenFisico
        {
            Id = Guid.NewGuid(),
            FechaHoraExamen = e.FechaHoraExamen,
            Lotep = e.Lotep,
            EstadoGeneral = e.EstadoGeneral,
            EstadoHidratacion = e.EstadoHidratacion,
            EstadoNutricion = e.EstadoNutricion,
            EscalaGlasgow = e.EscalaGlasgow,
            UteroGravido = e.UteroGravido,
            AlturaUterina = e.AlturaUterina,
            SituacionPosicionPresentacion = e.SituacionPosicionPresentacion,
            LatidosCardiacosFetales = e.LatidosCardiacosFetales,
            MovimientosFetales = e.MovimientosFetales,
            TonoUterino = e.TonoUterino,
            DinamicaUterina = e.DinamicaUterina,
            SangradoTv = e.SangradoTv,
            PerdidaLiquidoAmniotico = e.PerdidaLiquidoAmniotico,
            ColorLiquidoAmniotico = e.ColorLiquidoAmniotico,
            TaponMucoso = e.TaponMucoso,
            FlujoVaginal = e.FlujoVaginal,
            PunoPercusionLumbar = e.PunoPercusionLumbar,
            Edemas = e.Edemas,
            ReflejosOsteotendinosos = e.ReflejosOsteotendinosos
        }).ToList() ?? new List<ExamenFisico>();

        atencion.TactosVaginales = dto.TactosVaginales?.Select(t => new TactoVaginal
        {
            Id = Guid.NewGuid(),
            FechaHora = t.FechaHora,
            Dilatacion = t.Dilatacion,
            Borramiento = t.Borramiento,
            AlturaPresentacion = t.AlturaPresentacion,
            MembranasOvulares = t.MembranasOvulares,
            ColorLiquido = t.ColorLiquido,
            Pelvis = t.Pelvis,
            VariedadPresentacion = t.VariedadPresentacion
        }).ToList() ?? new List<TactoVaginal>();

        atencion.Ecografias = dto.Ecografias?.Select(e => new EcografiaObstetrica
        {
            Id = Guid.NewGuid(),
            FechaHora = e.FechaHora,
            DiametroBiparietal = e.DiametroBiparietal,
            CircunferenciaCefalica = e.CircunferenciaCefalica,
            CircunferenciaAbdominal = e.CircunferenciaAbdominal,
            LongitudFemur = e.LongitudFemur,
            PesoFetalEstimado = e.PesoFetalEstimado,
            IndiceLiquidoAmniotico = e.IndiceLiquidoAmniotico,
            PlacentaLocalizacion = e.PlacentaLocalizacion,
            PlacentaGranum = e.PlacentaGranum,
            CircularCordon = e.CircularCordon,
            Conclusiones = e.Conclusiones
        }).ToList() ?? new List<EcografiaObstetrica>();

        if (dto.ImpresionDiagnostica != null)
        {
            atencion.ImpresionDiagnostica = new ImpresionDiagnostica
            {
                Id = Guid.NewGuid(),
                DiagnosticoPrincipal = dto.ImpresionDiagnostica.DiagnosticoPrincipal,
                DiagnosticosSecundarios = dto.ImpresionDiagnostica.DiagnosticosSecundarios,
                IndicacionesReceta = dto.ImpresionDiagnostica.IndicacionesReceta,
                FechaProximaCita = dto.ImpresionDiagnostica.FechaProximaCita,
                MotivoProximaCita = dto.ImpresionDiagnostica.MotivoProximaCita
            };
        }

        // 3. Creación del Pago
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            CodigoPago = GenerarCodigo("PAG", paciente.DNI),
            AtencionId = atencion.Id,
            PacienteId = dto.PacienteId,
            ServicioClinicoId = dto.ServicioClinicoId,
            MontoTotal = dto.CostoFinal,
            MontoPagado = 0,
            SaldoPendiente = dto.CostoFinal,
            Estado = EstadoPago.Pendiente,
            FechaPago = DateTime.UtcNow,
            UsuarioRegistroId = usuarioId
        };

        // 4. Registro en el Historial Clínico
        var detalle = new HistorialDetalle
        {
            Id = Guid.NewGuid(),
            CodigoDetalle = GenerarCodigoDetalle(servicio.CodigoServicio),
            HistorialClinicoId = historialIdReal, // ✅ ERROR CORREGIDO AQUÍ TAMBIÉN
            AtencionId = atencion.Id,
            TipoMovimiento = TipoMovimientoHistorial.AtencionRegistrada,
            Titulo = "Apertura de Consulta Externa",
            Descripcion = dto.Anamnesis != null ? $"Motivo de consulta: {dto.Anamnesis.MotivoConsulta}" : "Atención aperturada sin anamnesis inicial.",
            FechaRegistro = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        // 5. Actualización de Cita
        if (dto.CitaId.HasValue)
        {
            var cita = await _citaRepository.GetByIdAsync(dto.CitaId.Value);
            if (cita != null)
            {
                cita.Estado = EstadoCita.EnProgreso;
                _citaRepository.Update(cita);
            }
        }

        await _atencionRepository.AddAsync(atencion);
        await _pagoRepository.AddAsync(pago);
        await _historialDetalleRepository.AddAsync(detalle);

        await _atencionRepository.SaveChangesAsync();

        return atencion.Id;
    }

    public async Task CerrarAtencionAsync(Guid id, CerrarAtencionDto dto)
    {
        var atencion = await _atencionRepository.ObtenerDetalleCompletoAsync(id);
        if (atencion == null) throw new KeyNotFoundException("Atención no encontrada.");

        if (atencion.Estado == EstadoAtencion.Cerrada)
            throw new InvalidOperationException("La atención ya está cerrada.");

        atencion.FechaCierre = DateTime.UtcNow;
        atencion.Estado = EstadoAtencion.Cerrada;

        if (atencion.ImpresionDiagnostica == null)
        {
            atencion.ImpresionDiagnostica = new ImpresionDiagnostica
            {
                Id = Guid.NewGuid(),
                AtencionId = atencion.Id,
                DiagnosticoPrincipal = dto.ImpresionDiagnostica.DiagnosticoPrincipal,
                DiagnosticosSecundarios = dto.ImpresionDiagnostica.DiagnosticosSecundarios,
                IndicacionesReceta = dto.ImpresionDiagnostica.IndicacionesReceta,
                FechaProximaCita = dto.ImpresionDiagnostica.FechaProximaCita,
                MotivoProximaCita = dto.ImpresionDiagnostica.MotivoProximaCita
            };
        }
        else
        {
            atencion.ImpresionDiagnostica.DiagnosticoPrincipal = dto.ImpresionDiagnostica.DiagnosticoPrincipal;
            atencion.ImpresionDiagnostica.DiagnosticosSecundarios = dto.ImpresionDiagnostica.DiagnosticosSecundarios;
            atencion.ImpresionDiagnostica.IndicacionesReceta = dto.ImpresionDiagnostica.IndicacionesReceta;
            atencion.ImpresionDiagnostica.FechaProximaCita = dto.ImpresionDiagnostica.FechaProximaCita;
            atencion.ImpresionDiagnostica.MotivoProximaCita = dto.ImpresionDiagnostica.MotivoProximaCita;
        }

        if (!string.IsNullOrEmpty(dto.ObservacionesFinales))
        {
            atencion.ImpresionDiagnostica.DiagnosticosSecundarios += $"\nOBSERVACIONES: {dto.ObservacionesFinales}";
        }

        if (atencion.CitaId.HasValue)
        {
            var cita = await _citaRepository.GetByIdAsync(atencion.CitaId.Value);
            if (cita != null)
            {
                cita.Estado = EstadoCita.Atendida;
                _citaRepository.Update(cita);
            }
        }

        _atencionRepository.Update(atencion);
        await _atencionRepository.SaveChangesAsync();
    }

    public async Task AnularAtencionAsync(Guid id, string motivo)
    {
        var atencion = await _atencionRepository.GetByIdAsync(id);
        if (atencion == null) throw new KeyNotFoundException("Atención no encontrada.");

        if (atencion.Estado == EstadoAtencion.Cerrada)
            throw new InvalidOperationException("No se puede anular una atención cerrada.");

        atencion.Estado = EstadoAtencion.Anulada;

        if (atencion.CitaId.HasValue)
        {
            var cita = await _citaRepository.GetByIdAsync(atencion.CitaId.Value);
            if (cita != null)
            {
                cita.Estado = EstadoCita.Cancelada;
                _citaRepository.Update(cita);
            }
        }

        _atencionRepository.Update(atencion);
        await _atencionRepository.SaveChangesAsync();
    }

    // ===================== MAPEO Y GENERADORES =====================

    private static AtencionResponseDto MapearAtencion(Atencion atencion)
    {
        return new AtencionResponseDto
        {
            Id = atencion.Id,
            CodigoAtencion = atencion.CodigoAtencion,
            PacienteId = atencion.PacienteId,
            PacienteNombre = $"{atencion.Paciente?.Nombres} {atencion.Paciente?.Apellidos}".Trim(),
            DoctorId = atencion.DoctorId,
            DoctorNombre = $"{atencion.Doctor?.Usuario?.Nombres} {atencion.Doctor?.Usuario?.Apellidos}".Trim(),
            ServicioClinicoId = atencion.ServicioClinicoId,
            ServicioNombre = atencion.ServicioClinico?.Nombre ?? string.Empty,
            CitaId = atencion.CitaId,
            HistorialClinicoId = atencion.HistorialClinicoId ?? Guid.Empty,
            FechaInicio = atencion.FechaInicio,
            FechaCierre = atencion.FechaCierre,
            Estado = atencion.Estado,

            CostoFinal = atencion.Pagos?.Sum(p => p.MontoTotal) ?? 0,
            MontoPagado = atencion.Pagos?.Sum(p => p.MontoPagado) ?? 0,
            SaldoPendiente = atencion.Pagos?.Sum(p => p.SaldoPendiente) ?? 0,

            // Mapeo modular independiente
            Anamnesis = atencion.Anamnesis != null ? MapearAnamnesis(atencion.Anamnesis) : null,
            ExamenesFisicos = atencion.ExamenesFisicos?.Select(MapearExamenFisico).ToList() ?? new List<ExamenFisicoDto>(),
            TactosVaginales = atencion.TactosVaginales?.Select(MapearTactoVaginal).ToList() ?? new List<TactoVaginalDto>(),
            Ecografias = atencion.Ecografias?.Select(MapearEcografia).ToList() ?? new List<EcografiaObstetricaDto>(),
            ImpresionDiagnostica = atencion.ImpresionDiagnostica != null ? MapearImpresionDiagnostica(atencion.ImpresionDiagnostica) : null
        };
    }

    private static AnamnesisDto MapearAnamnesis(Anamnesis an) => new()
    {
        MotivoConsulta = an.MotivoConsulta,
        Gestaciones = an.Gestaciones,
        HijosVivos = an.HijosVivos,
        Abortos = an.Abortos,
        PartosPretermino = an.PartosPretermino,
        PartosATermino = an.PartosATermino,
        FechaUltimaRegla = an.FechaUltimaRegla,
        FechaProbableParto = an.FechaProbableParto,
        EdadGestacional = an.EdadGestacional,
        Alergias = an.Alergias,
        EnfermedadesCronicas = an.EnfermedadesCronicas,
        CirugiasPrevias = an.CirugiasPrevias,
        AntecedentesAdicionales = an.AntecedentesAdicionales
    };

    private static ExamenFisicoDto MapearExamenFisico(ExamenFisico e) => new()
    {
        FechaHoraExamen = e.FechaHoraExamen,
        Lotep = e.Lotep,
        EstadoGeneral = e.EstadoGeneral,
        EstadoHidratacion = e.EstadoHidratacion,
        EstadoNutricion = e.EstadoNutricion,
        EscalaGlasgow = e.EscalaGlasgow,
        UteroGravido = e.UteroGravido,
        AlturaUterina = e.AlturaUterina,
        SituacionPosicionPresentacion = e.SituacionPosicionPresentacion,
        LatidosCardiacosFetales = e.LatidosCardiacosFetales,
        MovimientosFetales = e.MovimientosFetales,
        TonoUterino = e.TonoUterino,
        DinamicaUterina = e.DinamicaUterina,
        SangradoTv = e.SangradoTv,
        PerdidaLiquidoAmniotico = e.PerdidaLiquidoAmniotico,
        ColorLiquidoAmniotico = e.ColorLiquidoAmniotico,
        TaponMucoso = e.TaponMucoso,
        FlujoVaginal = e.FlujoVaginal,
        PunoPercusionLumbar = e.PunoPercusionLumbar,
        Edemas = e.Edemas,
        ReflejosOsteotendinosos = e.ReflejosOsteotendinosos
    };

    private static TactoVaginalDto MapearTactoVaginal(TactoVaginal t) => new()
    {
        FechaHora = t.FechaHora,
        Dilatacion = t.Dilatacion,
        Borramiento = t.Borramiento,
        AlturaPresentacion = t.AlturaPresentacion,
        MembranasOvulares = t.MembranasOvulares,
        ColorLiquido = t.ColorLiquido,
        Pelvis = t.Pelvis,
        VariedadPresentacion = t.VariedadPresentacion
    };

    private static EcografiaObstetricaDto MapearEcografia(EcografiaObstetrica e) => new()
    {
        FechaHora = e.FechaHora,
        DiametroBiparietal = e.DiametroBiparietal,
        CircunferenciaCefalica = e.CircunferenciaCefalica,
        CircunferenciaAbdominal = e.CircunferenciaAbdominal,
        LongitudFemur = e.LongitudFemur,
        PesoFetalEstimado = e.PesoFetalEstimado,
        IndiceLiquidoAmniotico = e.IndiceLiquidoAmniotico,
        PlacentaLocalizacion = e.PlacentaLocalizacion,
        PlacentaGranum = e.PlacentaGranum,
        CircularCordon = e.CircularCordon,
        Conclusiones = e.Conclusiones
    };

    private static ImpresionDiagnosticaDto MapearImpresionDiagnostica(ImpresionDiagnostica id) => new()
    {
        DiagnosticoPrincipal = id.DiagnosticoPrincipal,
        DiagnosticosSecundarios = id.DiagnosticosSecundarios,
        IndicacionesReceta = id.IndicacionesReceta,
        FechaProximaCita = id.FechaProximaCita,
        MotivoProximaCita = id.MotivoProximaCita
    };

    private static string GenerarCodigoAtencion(string codigoServicio)
    {
        return $"ATN-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{codigoServicio.ToUpper()}";
    }

    private static string GenerarCodigoDetalle(string codigoServicio)
    {
        return $"{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{codigoServicio}-{DateTime.UtcNow:yyyy}";
    }

    private static string GenerarCodigo(string prefijo, string dni)
    {
        return $"{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{prefijo}-{DateTime.UtcNow:yyyy}-{dni}";
    }
}