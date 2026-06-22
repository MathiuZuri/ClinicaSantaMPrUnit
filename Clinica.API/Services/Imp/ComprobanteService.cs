using System.Text.Json;
using Clinica.Domain.DTOs.Comprobantes;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class ComprobanteService : IComprobanteService
{
    private decimal TasaIgvActiva => (decimal)TasaImpuesto.IGV_18;

    private readonly IComprobanteRepository _comprobanteRepository;
    private readonly IPagoRepository _pagoRepository;
    private readonly IAtencionRepository _atencionRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    private readonly IComprobantePdfService _comprobantePdfService;
    private readonly ICitaRepository _citaRepository;
    private readonly IPacienteRepository _pacienteRepository;

    public ComprobanteService(
        IComprobanteRepository comprobanteRepository,
        IPagoRepository pagoRepository,
        IAtencionRepository atencionRepository,
        IUsuarioActualService usuarioActualService,
        IComprobantePdfService comprobantePdfService,
        ICitaRepository citaRepository,
        IPacienteRepository pacienteRepository)
    {
        _comprobanteRepository = comprobanteRepository;
        _pagoRepository = pagoRepository;
        _atencionRepository = atencionRepository;
        _usuarioActualService = usuarioActualService;
        _comprobantePdfService = comprobantePdfService;
        _citaRepository = citaRepository;
        _pacienteRepository = pacienteRepository;
    }

    // ==========================================================
    // 1. BOLETA DE PAGO (Preview, Emisión, PDF)
    // ==========================================================
    public async Task<ComprobantePagoPreviewDto> PreviewBoletaPagoAsync(Guid pagoId, decimal? tasaImpuesto = null)
    {
        if (pagoId == Guid.Empty)
            throw new InvalidOperationException("El identificador del pago es obligatorio.");
        
        var tasaFinal = tasaImpuesto ?? TasaIgvActiva; 

        var pago = await ObtenerPagoConDetallePorIdAsync(pagoId);

        var subtotal = CalcularSubtotalDesdeTotal(pago.MontoPagado, tasaFinal);
        var impuesto = pago.MontoPagado - subtotal;

        return new ComprobantePagoPreviewDto
        {
            CodigoComprobante = "PREVIEW",
            PagoId = pago.Id,
            CodigoPago = pago.CodigoPago,

            PacienteId = pago.PacienteId,
            Paciente = pago.Paciente == null ? "" : $"{pago.Paciente.Nombres} {pago.Paciente.Apellidos}",
            DniPaciente = pago.Paciente?.DNI ?? "",

            AtencionId = pago.AtencionId,
            CodigoAtencion = pago.Atencion?.CodigoAtencion,

            CitaId = pago.CitaId,
            CodigoCita = pago.Cita?.CodigoCita,

            Servicio = pago.ServicioClinico?.Nombre ?? "Servicio clínico",

            MontoPagado = pago.MontoPagado,
            Subtotal = subtotal,
            TasaImpuesto = tasaFinal,
            MontoImpuesto = impuesto,
            Total = pago.MontoPagado,

            MetodoPago = pago.MetodoPago.ToString(),
            EstadoPago = pago.Estado.ToString(),

            FechaPago = pago.FechaPago,
            FechaEmision = DateTime.UtcNow,

            Observacion = pago.Observacion,

            Detalles = new List<ComprobanteDetalleDto>
            {
                new()
                {
                    CodigoServicio = pago.ServicioClinico?.CodigoServicio ?? "",
                    Descripcion = pago.ServicioClinico?.Nombre ?? "Servicio clínico",
                    Cantidad = 1,
                    PrecioUnitarioFinal = pago.MontoPagado,
                    Subtotal = subtotal,
                    TasaImpuesto = tasaFinal,
                    MontoImpuesto = impuesto,
                    Total = pago.MontoPagado
                }
            }
        };
    }

    public async Task<Guid> EmitirBoletaPagoAsync(EmitirComprobantePagoDto dto)
    {
        if (dto.PagoId == Guid.Empty && string.IsNullOrWhiteSpace(dto.CodigoPago))
            throw new InvalidOperationException("Debe enviar el identificador del pago o el código de pago.");

        Pago pago;

        if (!string.IsNullOrWhiteSpace(dto.CodigoPago))
        {
            pago = await _pagoRepository.ObtenerPorCodigoConDetalleAsync(dto.CodigoPago.Trim())
                   ?? throw new KeyNotFoundException("Pago no encontrado.");
        }
        else
        {
            pago = await ObtenerPagoConDetallePorIdAsync(dto.PagoId);
        }

        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var serie = ObtenerSerie(TipoComprobante.BoletaPago);
        var ultimoNumero = await _comprobanteRepository.ObtenerUltimoNumeroPorSerieAsync(serie);
        var numero = ultimoNumero + 1;

        var subtotal = CalcularSubtotalDesdeTotal(pago.MontoPagado, TasaIgvActiva);
        var impuesto = pago.MontoPagado - subtotal;

        var comprobante = new Comprobante
        {
            Id = Guid.NewGuid(),
            CodigoComprobante = $"{serie}-{numero:000000}",
            Serie = serie,
            Numero = numero,

            TipoComprobante = TipoComprobante.BoletaPago,
            Estado = EstadoComprobante.Emitido,
            FormatoImpresion = TipoFormatoImpresion.A4,

            PacienteId = pago.PacienteId,
            PagoId = pago.Id,
            CitaId = pago.CitaId,
            AtencionId = pago.AtencionId,
            HistorialClinicoId = pago.Atencion?.HistorialClinicoId,

            TipoDocumentoPaciente = TipoDocumentoComprobante.DNI,
            NumeroDocumentoPaciente = pago.Paciente?.DNI ?? "",
            NombrePaciente = pago.Paciente == null ? "" : $"{pago.Paciente.Nombres} {pago.Paciente.Apellidos}",
            DireccionPaciente = pago.Paciente?.Direccion,

            Subtotal = subtotal,
            TasaImpuesto = TasaIgvActiva,
            MontoImpuesto = impuesto,
            Total = pago.MontoPagado,

            FechaEmision = DateTime.UtcNow,
            UsuarioEmisionId = usuarioId,

            Observacion = dto.Observacion?.Trim(),

            DatosSnapshotJson = JsonSerializer.Serialize(new
            {
                Tipo = "Boleta de pago",
                PagoId = pago.Id,
                CodigoPago = pago.CodigoPago,
                PacienteId = pago.PacienteId,
                Paciente = pago.Paciente == null ? "" : $"{pago.Paciente.Nombres} {pago.Paciente.Apellidos}",
                DniPaciente = pago.Paciente?.DNI ?? "",
                Servicio = pago.ServicioClinico?.Nombre ?? "Servicio clínico",
                MontoTotal = pago.MontoTotal,
                MontoPagado = pago.MontoPagado,
                SaldoPendiente = pago.SaldoPendiente,
                MetodoPago = pago.MetodoPago.ToString(),
                EstadoPago = pago.Estado.ToString(),
                FechaPago = pago.FechaPago,
                TasaImpuesto = TasaIgvActiva,
                Subtotal = subtotal,
                MontoImpuesto = impuesto,
                Total = pago.MontoPagado
            })
        };

        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Id = Guid.NewGuid(),
            ComprobanteId = comprobante.Id,

            CodigoServicio = pago.ServicioClinico?.CodigoServicio ?? "",
            Descripcion = pago.ServicioClinico?.Nombre ?? "Servicio clínico",
            Cantidad = 1,

            PrecioUnitarioFinal = pago.MontoPagado,
            Subtotal = subtotal,
            TasaImpuesto = TasaIgvActiva,
            MontoImpuesto = impuesto,
            Total = pago.MontoPagado
        });

        await _comprobanteRepository.AddAsync(comprobante);
        await _comprobanteRepository.SaveChangesAsync();

        return comprobante.Id;
    }

    public async Task<DocumentoGeneradoDto> GenerarPdfBoletaPagoAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.BoletaPago)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a una boleta de pago.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        var preview = MapearPagoPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarBoletaPagoPdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }

    // ==========================================================
    // 2. CONSTANCIA DE CITA (Preview, Emisión, PDF)
    // ==========================================================
    public async Task<ComprobanteCitaPreviewDto> PreviewConstanciaCitaAsync(Guid citaId)
    {
        if (citaId == Guid.Empty)
            throw new InvalidOperationException("El identificador de la cita es obligatorio.");

        var cita = await _citaRepository.ObtenerPorIdConRelacionesAsync(citaId)
            ?? throw new KeyNotFoundException("Cita no encontrada.");

        return new ComprobanteCitaPreviewDto
        {
            ComprobanteId = Guid.Empty,
            CodigoComprobante = "PREVIEW",

            CitaId = cita.Id,
            CodigoCita = cita.CodigoCita,

            PacienteId = cita.PacienteId,
            Paciente = $"{cita.Paciente.Nombres} {cita.Paciente.Apellidos}",
            DniPaciente = cita.Paciente.DNI,
            DireccionPaciente = cita.Paciente.Direccion,

            DoctorId = cita.DoctorId,
            Doctor = $"{cita.Doctor.Nombres} {cita.Doctor.Apellidos}",
            Especialidad = cita.Doctor.Especialidad,

            ServicioClinicoId = cita.ServicioClinicoId,
            Servicio = cita.ServicioClinico.Nombre,

            FechaCita = cita.Fecha,
            HoraInicio = cita.HoraInicio,
            HoraFin = cita.HoraFin,

            EstadoCita = cita.Estado.ToString(),
            Motivo = cita.Motivo,

            FechaEmision = DateTime.UtcNow,
            Observacion = "Vista previa"
        };
    }

    public async Task<Guid> EmitirConstanciaCitaAsync(EmitirComprobanteCitaDto dto)
    {
        if (dto.CitaId == Guid.Empty)
            throw new InvalidOperationException("El identificador de la cita es obligatorio.");

        var cita = await _citaRepository.ObtenerPorIdConRelacionesAsync(dto.CitaId)
            ?? throw new KeyNotFoundException("Cita no encontrada.");

        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var serie = ObtenerSerie(TipoComprobante.ConstanciaCita);
        var ultimoNumero = await _comprobanteRepository.ObtenerUltimoNumeroPorSerieAsync(serie);
        var numero = ultimoNumero + 1;

        var comprobante = new Comprobante
        {
            Id = Guid.NewGuid(),
            CodigoComprobante = $"{serie}-{numero:000000}",
            Serie = serie,
            Numero = numero,

            TipoComprobante = TipoComprobante.ConstanciaCita,
            Estado = EstadoComprobante.Emitido,
            FormatoImpresion = dto.FormatoImpresion,

            PacienteId = cita.PacienteId,
            CitaId = cita.Id,
            HistorialClinicoId = cita.Paciente.HistorialClinico?.Id,

            TipoDocumentoPaciente = TipoDocumentoComprobante.DNI,
            NumeroDocumentoPaciente = cita.Paciente.DNI,
            NombrePaciente = $"{cita.Paciente.Nombres} {cita.Paciente.Apellidos}",
            DireccionPaciente = cita.Paciente.Direccion,

            Subtotal = 0,
            TasaImpuesto = 0,
            MontoImpuesto = 0,
            Total = 0,

            FechaEmision = DateTime.UtcNow,
            UsuarioEmisionId = usuarioId,

            Observacion = dto.Observacion?.Trim(),

            DatosSnapshotJson = JsonSerializer.Serialize(new
            {
                Tipo = "Constancia de cita",
                CitaId = cita.Id,
                CodigoCita = cita.CodigoCita,
                PacienteId = cita.PacienteId,
                Paciente = $"{cita.Paciente.Nombres} {cita.Paciente.Apellidos}",
                DniPaciente = cita.Paciente.DNI,
                DoctorId = cita.DoctorId,
                Doctor = $"{cita.Doctor.Nombres} {cita.Doctor.Apellidos}",
                Servicio = cita.ServicioClinico.Nombre,
                Fecha = cita.Fecha,
                HoraInicio = cita.HoraInicio,
                HoraFin = cita.HoraFin,
                Motivo = cita.Motivo,
                EstadoCita = cita.Estado.ToString()
            })
        };

        await _comprobanteRepository.AddAsync(comprobante);
        await _comprobanteRepository.SaveChangesAsync();

        return comprobante.Id;
    }

    public async Task<DocumentoGeneradoDto> GenerarPdfConstanciaCitaAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.ConstanciaCita)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a una constancia de cita.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        var preview = MapearCitaPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarConstanciaCitaPdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }

    // ==========================================================
    // 3. RESUMEN DE ATENCIÓN (Preview, Emisión, PDF)
    // ==========================================================
    public async Task<ComprobanteAtencionPreviewDto> PreviewResumenAtencionAsync(Guid atencionId)
    {
        if (atencionId == Guid.Empty)
            throw new InvalidOperationException("El identificador de la atención es obligatorio.");

        var atencion = await _atencionRepository.ObtenerDetalleCompletoAsync(atencionId)
            ?? throw new KeyNotFoundException("Atención no encontrada.");

        return MapearAtencionPreview(atencion, "PREVIEW");
    }

    public async Task<Guid> EmitirResumenAtencionAsync(EmitirComprobanteAtencionDto dto)
    {
        if (dto.AtencionId == Guid.Empty)
            throw new InvalidOperationException("El identificador de la atención es obligatorio.");

        var atencion = await _atencionRepository.ObtenerDetalleCompletoAsync(dto.AtencionId)
            ?? throw new KeyNotFoundException("Atención no encontrada.");

        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var serie = ObtenerSerie(TipoComprobante.ResumenAtencion);
        var ultimoNumero = await _comprobanteRepository.ObtenerUltimoNumeroPorSerieAsync(serie);
        var numero = ultimoNumero + 1;

        var costoFinal = atencion.Pagos?.Sum(p => p.MontoTotal) ?? 0;
        var montoPagado = atencion.Pagos?.Sum(p => p.MontoPagado) ?? 0;
        var saldoPendiente = atencion.Pagos?.Sum(p => p.SaldoPendiente) ?? 0;
        var subtotal = CalcularSubtotalDesdeTotal(costoFinal, TasaIgvActiva);
        var impuesto = costoFinal - subtotal;

        var comprobante = new Comprobante
        {
            Id = Guid.NewGuid(),
            CodigoComprobante = $"{serie}-{numero:000000}",
            Serie = serie,
            Numero = numero,

            TipoComprobante = TipoComprobante.ResumenAtencion,
            Estado = EstadoComprobante.Emitido,
            FormatoImpresion = dto.FormatoImpresion,

            PacienteId = atencion.PacienteId,
            AtencionId = atencion.Id,
            HistorialClinicoId = atencion.HistorialClinicoId,

            TipoDocumentoPaciente = TipoDocumentoComprobante.DNI,
            NumeroDocumentoPaciente = atencion.Paciente.DNI,
            NombrePaciente = $"{atencion.Paciente.Nombres} {atencion.Paciente.Apellidos}",
            DireccionPaciente = atencion.Paciente.Direccion,

            Subtotal = subtotal,
            TasaImpuesto = TasaIgvActiva,
            MontoImpuesto = impuesto,
            Total = costoFinal,

            FechaEmision = DateTime.UtcNow,
            UsuarioEmisionId = usuarioId,

            Observacion = dto.Observacion?.Trim(),

            DatosSnapshotJson = JsonSerializer.Serialize(new
            {
                Tipo = "Resumen de atención",
                AtencionId = atencion.Id,
                CodigoAtencion = atencion.CodigoAtencion,
                PacienteId = atencion.PacienteId,
                Paciente = $"{atencion.Paciente.Nombres} {atencion.Paciente.Apellidos}",
                Doctor = $"{atencion.Doctor.Nombres} {atencion.Doctor.Apellidos}",
                Servicio = atencion.ServicioClinico.Nombre,
                // ✅ Volvemos a leer de los módulos independientes
                MotivoConsulta = atencion.Anamnesis?.MotivoConsulta,
                DiagnosticoPrincipal = atencion.ImpresionDiagnostica?.DiagnosticoPrincipal,
                FechaInicio = atencion.FechaInicio,
                FechaCierre = atencion.FechaCierre,
                CostoFinal = costoFinal,
                MontoPagado = montoPagado,
                SaldoPendiente = saldoPendiente
            })
        };

        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Id = Guid.NewGuid(),
            ComprobanteId = comprobante.Id,
            CodigoServicio = atencion.ServicioClinico.CodigoServicio,
            Descripcion = atencion.ServicioClinico.Nombre,
            Cantidad = 1,
            PrecioUnitarioFinal = costoFinal,
            Subtotal = subtotal,
            TasaImpuesto = TasaIgvActiva,
            MontoImpuesto = impuesto,
            Total = costoFinal
        });

        await _comprobanteRepository.AddAsync(comprobante);
        await _comprobanteRepository.SaveChangesAsync();

        return comprobante.Id;
    }

    public async Task<DocumentoGeneradoDto> GenerarPdfResumenAtencionAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.ResumenAtencion)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a un resumen de atención.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        if (comprobante.AtencionId.HasValue)
        {
            comprobante.Atencion = await _atencionRepository.ObtenerDetalleCompletoAsync(comprobante.AtencionId.Value);
        }

        var preview = MapearAtencionPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarResumenAtencionPdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }

    // ==========================================================
    // 4. ESTADO DE CUENTA DEL PACIENTE (Preview, Emisión, PDF)
    // ==========================================================
    public async Task<ComprobanteEstadoCuentaPreviewDto> PreviewEstadoCuentaPacienteAsync(Guid pacienteId)
    {
        if (pacienteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del paciente es obligatorio.");

        var paciente = await _pacienteRepository.GetByIdAsync(pacienteId)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        var pagos = await _pagoRepository.ObtenerPorPacienteAsync(pacienteId);

        var pagosValidos = pagos
            .Where(x => x.Estado != EstadoPago.Anulado && x.Estado != EstadoPago.Eliminado)
            .OrderByDescending(x => x.FechaPago)
            .ToList();

        var totalFacturado = pagosValidos.Sum(x => x.MontoTotal);
        var totalPagado = pagosValidos.Sum(x => x.MontoPagado);
        var totalPendiente = Math.Max(totalFacturado - totalPagado, 0);

        return new ComprobanteEstadoCuentaPreviewDto
        {
            ComprobanteId = Guid.Empty,
            CodigoComprobante = "PREVIEW",

            PacienteId = paciente.Id,
            Paciente = $"{paciente.Nombres} {paciente.Apellidos}",
            DniPaciente = paciente.DNI,
            DireccionPaciente = paciente.Direccion,

            TotalFacturado = totalFacturado,
            TotalPagado = totalPagado,
            TotalPendiente = totalPendiente,

            FechaEmision = DateTime.UtcNow,

            Detalles = pagosValidos.Select(x => new DetalleEstadoCuentaComprobanteDto
            {
                PagoId = x.Id,
                CodigoPago = x.CodigoPago,
                Servicio = x.ServicioClinico?.Nombre ?? "",
                FechaPago = x.FechaPago,
                MontoTotal = x.MontoTotal,
                MontoPagado = x.MontoPagado,
                SaldoPendiente = x.SaldoPendiente,
                EstadoPago = x.Estado.ToString()
            }).ToList()
        };
    }

    public async Task<Guid> EmitirEstadoCuentaPacienteAsync(EmitirComprobanteEstadoCuentaDto dto)
    {
        if (dto.PacienteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del paciente es obligatorio.");

        var paciente = await _pacienteRepository.GetByIdAsync(dto.PacienteId)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        var pagos = await _pagoRepository.ObtenerPorPacienteAsync(dto.PacienteId);
        var pagosValidos = pagos
            .Where(x => x.Estado != EstadoPago.Anulado && x.Estado != EstadoPago.Eliminado)
            .OrderByDescending(x => x.FechaPago)
            .ToList();

        var totalFacturado = pagosValidos.Sum(x => x.MontoTotal);
        var totalPagado = pagosValidos.Sum(x => x.MontoPagado);
        var totalPendiente = Math.Max(totalFacturado - totalPagado, 0);

        var usuarioId = _usuarioActualService.ObtenerUsuarioId();
        var serie = ObtenerSerie(TipoComprobante.EstadoCuenta);
        var ultimoNumero = await _comprobanteRepository.ObtenerUltimoNumeroPorSerieAsync(serie);
        var numero = ultimoNumero + 1;

        var comprobante = new Comprobante
        {
            Id = Guid.NewGuid(),
            CodigoComprobante = $"{serie}-{numero:000000}",
            Serie = serie,
            Numero = numero,

            TipoComprobante = TipoComprobante.EstadoCuenta,
            Estado = EstadoComprobante.Emitido,
            FormatoImpresion = dto.FormatoImpresion,

            PacienteId = paciente.Id,
            HistorialClinicoId = paciente.HistorialClinico?.Id,

            TipoDocumentoPaciente = TipoDocumentoComprobante.DNI,
            NumeroDocumentoPaciente = paciente.DNI,
            NombrePaciente = $"{paciente.Nombres} {paciente.Apellidos}",
            DireccionPaciente = paciente.Direccion,

            Subtotal = totalFacturado,
            TasaImpuesto = 0,
            MontoImpuesto = 0,
            Total = totalFacturado,

            FechaEmision = DateTime.UtcNow,
            UsuarioEmisionId = usuarioId,

            Observacion = dto.Observacion?.Trim(),

            DatosSnapshotJson = JsonSerializer.Serialize(new
            {
                Tipo = "Estado de cuenta",
                PacienteId = paciente.Id,
                Paciente = $"{paciente.Nombres} {paciente.Apellidos}",
                DniPaciente = paciente.DNI,
                TotalFacturado = totalFacturado,
                TotalPagado = totalPagado,
                TotalPendiente = totalPendiente,
                FechaEmision = DateTime.UtcNow
            })
        };

        foreach (var pago in pagosValidos)
        {
            comprobante.Detalles.Add(new ComprobanteDetalle
            {
                Id = Guid.NewGuid(),
                ComprobanteId = comprobante.Id,
                CodigoServicio = pago.ServicioClinico?.CodigoServicio ?? "",
                Descripcion = $"{pago.ServicioClinico?.Nombre ?? "Servicio"} - {pago.CodigoPago}",
                Cantidad = 1,
                PrecioUnitarioFinal = pago.MontoTotal,
                Subtotal = pago.MontoTotal,
                TasaImpuesto = 0,
                MontoImpuesto = 0,
                Total = pago.MontoTotal
            });
        }

        await _comprobanteRepository.AddAsync(comprobante);
        await _comprobanteRepository.SaveChangesAsync();

        return comprobante.Id;
    }

    public async Task<DocumentoGeneradoDto> GenerarPdfEstadoCuentaPacienteAsync(Guid comprobanteId)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.TipoComprobante != TipoComprobante.EstadoCuenta)
            throw new InvalidOperationException("El comprobante solicitado no corresponde a un estado de cuenta.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede generar PDF de un comprobante anulado.");

        var preview = MapearEstadoCuentaPreview(comprobante);
        var archivo = _comprobantePdfService.GenerarEstadoCuentaPacientePdf(preview);

        return new DocumentoGeneradoDto
        {
            NombreArchivo = $"{comprobante.CodigoComprobante}.pdf",
            ContentType = "application/pdf",
            Archivo = archivo
        };
    }

    // ==========================================================
    // 5. CONSULTAS Y ANULACIÓN
    // ==========================================================
    public async Task<ComprobanteDto> ObtenerPorIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        var comprobante = await _comprobanteRepository.ObtenerPorIdConDetalleAsync(id)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        return MapearComprobante(comprobante);
    }

    public async Task<IEnumerable<ComprobanteDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        if (pacienteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del paciente es obligatorio.");

        var comprobantes = await _comprobanteRepository.ObtenerPorPacienteAsync(pacienteId);
        return comprobantes.Select(MapearComprobante).ToList();
    }

    public async Task<IEnumerable<ComprobanteDto>> ObtenerPorPagoAsync(Guid pagoId)
    {
        if (pagoId == Guid.Empty)
            throw new InvalidOperationException("El identificador del pago es obligatorio.");

        var comprobantes = await _comprobanteRepository.ObtenerPorPagoAsync(pagoId);
        return comprobantes.Select(MapearComprobante).ToList();
    }

    public async Task<IEnumerable<ComprobanteDto>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        if (atencionId == Guid.Empty)
            throw new InvalidOperationException("El identificador de la atención es obligatorio.");

        var comprobantes = await _comprobanteRepository.ObtenerPorAtencionAsync(atencionId);
        return comprobantes.Select(MapearComprobante).ToList();
    }

    public async Task AnularComprobanteAsync(Guid comprobanteId, string motivo)
    {
        if (comprobanteId == Guid.Empty)
            throw new InvalidOperationException("El identificador del comprobante es obligatorio.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new InvalidOperationException("El motivo de anulación es obligatorio.");

        var comprobante = await _comprobanteRepository.GetByIdAsync(comprobanteId)
            ?? throw new KeyNotFoundException("Comprobante no encontrado.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("El comprobante ya se encuentra anulado.");

        comprobante.Estado = EstadoComprobante.Anulado;
        comprobante.FechaAnulacion = DateTime.UtcNow;
        comprobante.UsuarioAnulacionId = _usuarioActualService.ObtenerUsuarioId();
        comprobante.MotivoAnulacion = motivo.Trim();

        _comprobanteRepository.Update(comprobante);
        await _comprobanteRepository.SaveChangesAsync();
    }

    // ==========================================================
    // 6. MÉTODOS PRIVADOS (Helpers)
    // ==========================================================
    private async Task<Pago> ObtenerPagoConDetallePorIdAsync(Guid pagoId)
    {
        var pagos = await _pagoRepository.ObtenerTodosConDetalleAsync();
        return pagos.FirstOrDefault(x => x.Id == pagoId)
               ?? throw new KeyNotFoundException("Pago no encontrado.");
    }

    private static string ObtenerSerie(TipoComprobante tipo)
    {
        return tipo switch
        {
            TipoComprobante.BoletaPago => "B001",
            TipoComprobante.ConstanciaCita => "C001",
            TipoComprobante.ResumenAtencion => "A001",
            TipoComprobante.EstadoCuenta => "E001",
            TipoComprobante.HistoriaClinica => "H001",
            _ => "D001"
        };
    }

    private static decimal CalcularSubtotalDesdeTotal(decimal total, decimal tasaImpuesto)
    {
        return Math.Round(total / (1 + tasaImpuesto / 100), 2);
    }

    // --- Mapeadores de Preview ---

    private static ComprobanteAtencionPreviewDto MapearAtencionPreview(Atencion atencion, string codigoComprobante)
    {
        var costoFinal = atencion.Pagos?.Sum(p => p.MontoTotal) ?? 0;
        var montoPagado = atencion.Pagos?.Sum(p => p.MontoPagado) ?? 0;
        var saldoPendiente = atencion.Pagos?.Sum(p => p.SaldoPendiente) ?? 0;

        return new ComprobanteAtencionPreviewDto
        {
            ComprobanteId = Guid.Empty,
            CodigoComprobante = codigoComprobante,

            AtencionId = atencion.Id,
            CodigoAtencion = atencion.CodigoAtencion ?? "",

            PacienteId = atencion.PacienteId,
            Paciente = $"{atencion.Paciente.Nombres} {atencion.Paciente.Apellidos}",
            DniPaciente = atencion.Paciente.DNI,
            DireccionPaciente = atencion.Paciente.Direccion,

            DoctorId = atencion.DoctorId,
            Doctor = atencion.Doctor == null ? "" : $"{atencion.Doctor.Nombres} {atencion.Doctor.Apellidos}",
            Especialidad = atencion.Doctor?.Especialidad ?? "",

            ServicioClinicoId = atencion.ServicioClinicoId,
            Servicio = atencion.ServicioClinico?.Nombre ?? "Servicio clínico",

            FechaInicio = atencion.FechaInicio,
            FechaCierre = atencion.FechaCierre,

            // ✅ Actualizado a la lectura de los módulos separados
            MotivoConsulta = atencion.Anamnesis?.MotivoConsulta ?? "",
            DiagnosticoResumen = atencion.ImpresionDiagnostica?.DiagnosticoPrincipal,
            Indicaciones = atencion.ImpresionDiagnostica?.IndicacionesReceta,
            Tratamiento = atencion.ImpresionDiagnostica?.DiagnosticosSecundarios,
            Observaciones = atencion.ImpresionDiagnostica?.DiagnosticosSecundarios,

            EstadoAtencion = atencion.Estado.ToString(),

            CostoFinal = costoFinal,
            MontoPagado = montoPagado,
            SaldoPendiente = saldoPendiente,

            FechaEmision = DateTime.UtcNow
        };
    }

    private static ComprobantePagoPreviewDto MapearPagoPreview(Comprobante comprobante)
    {
        return new ComprobantePagoPreviewDto
        {
            CodigoComprobante = comprobante.CodigoComprobante,

            PagoId = comprobante.PagoId ?? Guid.Empty,
            CodigoPago = comprobante.Pago?.CodigoPago ?? "",

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,

            AtencionId = comprobante.AtencionId,
            CodigoAtencion = comprobante.Atencion?.CodigoAtencion,

            CitaId = comprobante.CitaId,
            CodigoCita = comprobante.Cita?.CodigoCita,

            Servicio = comprobante.Detalles.FirstOrDefault()?.Descripcion ?? "Servicio clínico",

            MontoPagado = comprobante.Total,
            Subtotal = comprobante.Subtotal,
            TasaImpuesto = comprobante.TasaImpuesto,
            MontoImpuesto = comprobante.MontoImpuesto,
            Total = comprobante.Total,

            MetodoPago = comprobante.Pago?.MetodoPago.ToString() ?? "",
            EstadoPago = comprobante.Pago?.Estado.ToString() ?? "",

            FechaPago = comprobante.Pago?.FechaPago ?? comprobante.FechaEmision,
            FechaEmision = comprobante.FechaEmision,

            Observacion = comprobante.Observacion,

            Detalles = comprobante.Detalles.Select(d => new ComprobanteDetalleDto
            {
                Id = d.Id,
                CodigoServicio = d.CodigoServicio,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitarioFinal = d.PrecioUnitarioFinal,
                Subtotal = d.Subtotal,
                TasaImpuesto = d.TasaImpuesto,
                MontoImpuesto = d.MontoImpuesto,
                Total = d.Total
            }).ToList()
        };
    }

    private static ComprobanteCitaPreviewDto MapearCitaPreview(Comprobante comprobante)
    {
        var cita = comprobante.Cita;
        return new ComprobanteCitaPreviewDto
        {
            ComprobanteId = comprobante.Id,
            CodigoComprobante = comprobante.CodigoComprobante,

            CitaId = comprobante.CitaId ?? Guid.Empty,
            CodigoCita = cita?.CodigoCita ?? "",

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,
            DireccionPaciente = comprobante.DireccionPaciente,

            DoctorId = cita?.DoctorId ?? Guid.Empty,
            Doctor = cita?.Doctor == null ? "" : $"{cita.Doctor.Nombres} {cita.Doctor.Apellidos}",
            Especialidad = cita?.Doctor?.Especialidad ?? "",

            ServicioClinicoId = cita?.ServicioClinicoId ?? Guid.Empty,
            Servicio = cita?.ServicioClinico?.Nombre ?? "Servicio clínico",

            FechaCita = cita?.Fecha ?? default,
            HoraInicio = cita?.HoraInicio ?? default,
            HoraFin = cita?.HoraFin ?? default,

            EstadoCita = cita?.Estado.ToString() ?? "",
            Motivo = cita?.Motivo ?? "",

            FechaEmision = comprobante.FechaEmision,
            Observacion = comprobante.Observacion
        };
    }

    private static ComprobanteAtencionPreviewDto MapearAtencionPreview(Comprobante comprobante)
    {
        var atencion = comprobante.Atencion;
        var costoFinal = atencion?.Pagos?.Sum(p => p.MontoTotal) ?? 0;
        var montoPagado = atencion?.Pagos?.Sum(p => p.MontoPagado) ?? 0;
        var saldoPendiente = atencion?.Pagos?.Sum(p => p.SaldoPendiente) ?? 0;

        return new ComprobanteAtencionPreviewDto
        {
            ComprobanteId = comprobante.Id,
            CodigoComprobante = comprobante.CodigoComprobante,

            AtencionId = comprobante.AtencionId ?? Guid.Empty,
            CodigoAtencion = atencion?.CodigoAtencion ?? "",

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,
            DireccionPaciente = comprobante.DireccionPaciente,

            DoctorId = atencion?.DoctorId ?? Guid.Empty,
            Doctor = atencion?.Doctor == null ? "" : $"{atencion.Doctor.Nombres} {atencion.Doctor.Apellidos}",
            Especialidad = atencion?.Doctor?.Especialidad ?? "",

            ServicioClinicoId = atencion?.ServicioClinicoId ?? Guid.Empty,
            Servicio = atencion?.ServicioClinico?.Nombre ?? "Servicio clínico",

            FechaInicio = atencion?.FechaInicio ?? comprobante.FechaEmision,
            FechaCierre = atencion?.FechaCierre,

            // ✅ Actualizado a la lectura de los módulos separados
            MotivoConsulta = atencion?.Anamnesis?.MotivoConsulta ?? "",
            DiagnosticoResumen = atencion?.ImpresionDiagnostica?.DiagnosticoPrincipal,
            Indicaciones = atencion?.ImpresionDiagnostica?.IndicacionesReceta,
            Tratamiento = atencion?.ImpresionDiagnostica?.DiagnosticosSecundarios,
            Observaciones = atencion?.ImpresionDiagnostica?.DiagnosticosSecundarios,

            EstadoAtencion = atencion?.Estado.ToString() ?? "",

            CostoFinal = costoFinal,
            MontoPagado = montoPagado,
            SaldoPendiente = saldoPendiente,

            FechaEmision = comprobante.FechaEmision
        };
    }

    private static ComprobanteEstadoCuentaPreviewDto MapearEstadoCuentaPreview(Comprobante comprobante)
    {
        return new ComprobanteEstadoCuentaPreviewDto
        {
            ComprobanteId = comprobante.Id,
            CodigoComprobante = comprobante.CodigoComprobante,

            PacienteId = comprobante.PacienteId,
            Paciente = comprobante.NombrePaciente,
            DniPaciente = comprobante.NumeroDocumentoPaciente,
            DireccionPaciente = comprobante.DireccionPaciente,

            TotalFacturado = comprobante.Total,
            TotalPagado = comprobante.Total,
            TotalPendiente = 0,

            FechaEmision = comprobante.FechaEmision,

            Detalles = new List<DetalleEstadoCuentaComprobanteDto>()
        };
    }

    private static ComprobanteDto MapearComprobante(Comprobante x)
    {
        return new ComprobanteDto
        {
            Id = x.Id,
            CodigoComprobante = x.CodigoComprobante,
            Serie = x.Serie,
            Numero = x.Numero,

            TipoComprobante = x.TipoComprobante.ToString(),
            Estado = x.Estado.ToString(),
            FormatoImpresion = x.FormatoImpresion.ToString(),

            PacienteId = x.PacienteId,
            Paciente = x.NombrePaciente,

            TipoDocumentoPaciente = x.TipoDocumentoPaciente.ToString(),
            NumeroDocumentoPaciente = x.NumeroDocumentoPaciente,
            DireccionPaciente = x.DireccionPaciente,

            PagoId = x.PagoId,
            CitaId = x.CitaId,
            AtencionId = x.AtencionId,
            HistorialClinicoId = x.HistorialClinicoId,

            Subtotal = x.Subtotal,
            TasaImpuesto = x.TasaImpuesto,
            MontoImpuesto = x.MontoImpuesto,
            Total = x.Total,

            FechaEmision = x.FechaEmision,

            UsuarioEmisionId = x.UsuarioEmisionId,
            UsuarioEmision = x.UsuarioEmision == null ? null : $"{x.UsuarioEmision.Nombres} {x.UsuarioEmision.Apellidos}",

            FechaAnulacion = x.FechaAnulacion,
            UsuarioAnulacionId = x.UsuarioAnulacionId,
            UsuarioAnulacion = x.UsuarioAnulacion == null ? null : $"{x.UsuarioAnulacion.Nombres} {x.UsuarioAnulacion.Apellidos}",

            Observacion = x.Observacion,
            MotivoAnulacion = x.MotivoAnulacion,

            Detalles = x.Detalles.Select(d => new ComprobanteDetalleDto
            {
                Id = d.Id,
                CodigoServicio = d.CodigoServicio,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitarioFinal = d.PrecioUnitarioFinal,
                Subtotal = d.Subtotal,
                TasaImpuesto = d.TasaImpuesto,
                MontoImpuesto = d.MontoImpuesto,
                Total = d.Total
            }).ToList()
        };
    }
    
    public async Task<IEnumerable<ComprobanteDto>> ObtenerTodosAsync()
    {
        // Como actualmente no hay un método en el repositorio para obtener todos,
        // puedes usar el repositorio de Comprobante y obtener todos con un GetQueryable() o similar.
        // Asumo que tienes un método en IComprobanteRepository para obtener todos.
        var comprobantes = await _comprobanteRepository.GetAllAsync(); 
        // Si no existe, podrías hacer una consulta LINQ usando _comprobanteRepository.GetQueryable()
        return comprobantes.Select(MapearComprobante).ToList();
    }
}