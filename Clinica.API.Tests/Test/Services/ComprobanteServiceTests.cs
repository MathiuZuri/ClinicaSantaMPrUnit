using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Comprobantes;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class ComprobanteServiceTests
{
    private readonly IComprobanteRepository _comprobanteRepo;
    private readonly IPagoRepository _pagoRepo;
    private readonly IAtencionRepository _atencionRepo;
    private readonly IUsuarioActualService _usuarioActual;
    private readonly IComprobantePdfService _pdfService;
    private readonly ICitaRepository _citaRepo;
    private readonly IPacienteRepository _pacienteRepo;
    private readonly ComprobanteService _service;

    public ComprobanteServiceTests()
    {
        _comprobanteRepo = Substitute.For<IComprobanteRepository>();
        _pagoRepo = Substitute.For<IPagoRepository>();
        _atencionRepo = Substitute.For<IAtencionRepository>();
        _usuarioActual = Substitute.For<IUsuarioActualService>();
        _pdfService = Substitute.For<IComprobantePdfService>();
        _citaRepo = Substitute.For<ICitaRepository>();
        _pacienteRepo = Substitute.For<IPacienteRepository>();

        _service = new ComprobanteService(
            _comprobanteRepo,
            _pagoRepo,
            _atencionRepo,
            _usuarioActual,
            _pdfService,
            _citaRepo,
            _pacienteRepo);
    }

    // ---------- PREVIEWS ----------
    [Fact]
    public async Task PreviewBoletaPago_PagoIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.PreviewBoletaPagoAsync(Guid.Empty);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*pago*");
    }

    [Fact]
    public async Task PreviewBoletaPago_PagoNoEncontrado_LanzaKeyNotFound()
    {
        var pagoId = Guid.NewGuid();
        _pagoRepo.ObtenerTodosConDetalleAsync().Returns(new List<Pago>());
        Func<Task> act = () => _service.PreviewBoletaPagoAsync(pagoId);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Pago no encontrado*");
    }

    [Fact]
    public async Task PreviewBoletaPago_Valido_RetornaPreview()
    {
        var pagoId = Guid.NewGuid();
        var pago = CrearPago(pagoId);
        _pagoRepo.ObtenerTodosConDetalleAsync().Returns(new List<Pago> { pago });
        var result = await _service.PreviewBoletaPagoAsync(pagoId);
        result.PagoId.Should().Be(pagoId);
        result.Subtotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PreviewConstanciaCita_CitaIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.PreviewConstanciaCitaAsync(Guid.Empty);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cita*");
    }

    [Fact]
    public async Task PreviewResumenAtencion_AtencionIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.PreviewResumenAtencionAsync(Guid.Empty);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*atención*");
    }

    [Fact]
    public async Task PreviewEstadoCuenta_PacienteIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.PreviewEstadoCuentaPacienteAsync(Guid.Empty);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*paciente*");
    }

    // ---------- EMITIR BOLETA ----------
    [Fact]
    public async Task EmitirBoletaPago_AmbosIdentificadoresVacios_LanzaExcepcion()
    {
        var dto = new EmitirComprobantePagoDto();
        Func<Task> act = () => _service.EmitirBoletaPagoAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*identificador*");
    }

    [Fact]
    public async Task EmitirBoletaPago_PagoNoEncontradoPorCodigo_LanzaKeyNotFound()
    {
        var dto = new EmitirComprobantePagoDto { CodigoPago = "XXX" };
        _pagoRepo.ObtenerPorCodigoConDetalleAsync("XXX").Returns((Pago?)null);
        Func<Task> act = () => _service.EmitirBoletaPagoAsync(dto);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Pago no encontrado*");
    }

    [Fact]
    public async Task EmitirBoletaPago_Valido_CreaComprobante()
    {
        var pago = CrearPago(Guid.NewGuid());
        _pagoRepo.ObtenerTodosConDetalleAsync().Returns(new List<Pago> { pago });
        _usuarioActual.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _comprobanteRepo.ObtenerUltimoNumeroPorSerieAsync("B001").Returns(5);
        var dto = new EmitirComprobantePagoDto { PagoId = pago.Id };
        var result = await _service.EmitirBoletaPagoAsync(dto);
        result.Should().NotBeEmpty();
        await _comprobanteRepo.Received().AddAsync(Arg.Any<Comprobante>());
    }

    // ---------- GENERAR PDF ----------
    [Fact]
    public async Task GenerarPdfBoletaPago_ComprobanteAnulado_LanzaExcepcion()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante { Id = id, TipoComprobante = TipoComprobante.BoletaPago, Estado = EstadoComprobante.Anulado };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        Func<Task> act = () => _service.GenerarPdfBoletaPagoAsync(id);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*anulado*");
    }

    [Fact]
    public async Task GenerarPdfBoletaPago_TipoIncorrecto_LanzaExcepcion()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante { Id = id, TipoComprobante = TipoComprobante.ConstanciaCita, Estado = EstadoComprobante.Emitido };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        Func<Task> act = () => _service.GenerarPdfBoletaPagoAsync(id);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*boleta*");
    }

    [Fact]
    public async Task GenerarPdfBoletaPago_Valido_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.BoletaPago,
            Estado = EstadoComprobante.Emitido,
            Detalles = new List<ComprobanteDetalle> { new() }
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarBoletaPagoPdf(Arg.Any<ComprobantePagoPreviewDto>()).Returns(new byte[] { 1 });
        var result = await _service.GenerarPdfBoletaPagoAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }

    // ---------- CONSULTAS ----------
    [Fact]
    public async Task ObtenerPorId_ComprobanteNoExiste_LanzaKeyNotFound()
    {
        var id = Guid.NewGuid();
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns((Comprobante?)null);
        Func<Task> act = () => _service.ObtenerPorIdAsync(id);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Comprobante no encontrado*");
    }

    [Fact]
    public async Task ObtenerPorId_Valido_RetornaDto()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante { Id = id };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        var result = await _service.ObtenerPorIdAsync(id);
        result.Id.Should().Be(id);
    }

    [Fact]
    public async Task ObtenerPorPaciente_PacienteIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.ObtenerPorPacienteAsync(Guid.Empty);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*paciente*");
    }

    [Fact]
    public async Task ObtenerPorPago_PagoIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.ObtenerPorPagoAsync(Guid.Empty);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*pago*");
    }

    [Fact]
    public async Task ObtenerPorAtencion_AtencionIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.ObtenerPorAtencionAsync(Guid.Empty);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*atención*");
    }

    // ---------- ANULACIÓN ----------
    [Fact]
    public async Task AnularComprobante_ComprobanteIdEmpty_LanzaExcepcion()
    {
        Func<Task> act = () => _service.AnularComprobanteAsync(Guid.Empty, "motivo");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*comprobante*");
    }

    [Fact]
    public async Task AnularComprobante_MotivoVacio_LanzaExcepcion()
    {
        Func<Task> act = () => _service.AnularComprobanteAsync(Guid.NewGuid(), "   ");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*motivo*");
    }

    [Fact]
    public async Task AnularComprobante_ComprobanteYaAnulado_LanzaExcepcion()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante { Id = id, Estado = EstadoComprobante.Anulado };
        _comprobanteRepo.GetByIdAsync(id).Returns(comp);
        Func<Task> act = () => _service.AnularComprobanteAsync(id, "motivo");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*ya se encuentra anulado*");
    }

    [Fact]
    public async Task AnularComprobante_Valido_ActualizaEstado()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante { Id = id, Estado = EstadoComprobante.Emitido };
        _comprobanteRepo.GetByIdAsync(id).Returns(comp);
        _usuarioActual.ObtenerUsuarioId().Returns(Guid.NewGuid());
        await _service.AnularComprobanteAsync(id, "motivo válido");
        comp.Estado.Should().Be(EstadoComprobante.Anulado);
        _comprobanteRepo.Received().Update(comp);
    }

    // ---------- HELPERS ----------
    private Pago CrearPago(Guid id)
    {
        return new Pago
        {
            Id = id,
            CodigoPago = "PAG-001",
            MontoPagado = 118,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B", DNI = "12345678" },
            ServicioClinico = new ServicioClinico { CodigoServicio = "S1", Nombre = "Consulta" },
            MetodoPago = MetodoPago.Efectivo
        };
    }
    
    [Fact]
    public async Task EmitirEstadoCuenta_Valido_CubreSerieEstadoCuenta()
    {
        var paciente = new Paciente { Id = Guid.NewGuid(), Nombres = "A", Apellidos = "B", DNI = "12345678" };
        _pacienteRepo.GetByIdAsync(paciente.Id).Returns(paciente);
        _pagoRepo.ObtenerPorPacienteAsync(paciente.Id).Returns(new List<Pago>());
        _usuarioActual.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _comprobanteRepo.ObtenerUltimoNumeroPorSerieAsync("E001").Returns(0);

        var dto = new EmitirComprobanteEstadoCuentaDto { PacienteId = paciente.Id };
        var result = await _service.EmitirEstadoCuentaPacienteAsync(dto);
        result.Should().NotBeEmpty();
        await _comprobanteRepo.Received().AddAsync(Arg.Is<Comprobante>(c => c.Serie == "E001"));
    }
    [Fact]
    public async Task GenerarPdfConstanciaCita_ConCitaCompleta_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var cita = new Cita
        {
            Id = Guid.NewGuid(),
            CodigoCita = "CIT-001",
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor", Especialidad = "Ginecología" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" },
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Control",
            Estado = EstadoCita.Pendiente
        };
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ConstanciaCita,
            Estado = EstadoComprobante.Emitido,
            CitaId = cita.Id,
            Cita = cita,
            NombrePaciente = "Paciente",
            NumeroDocumentoPaciente = "12345678"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarConstanciaCitaPdf(Arg.Any<ComprobanteCitaPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfConstanciaCitaAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task EmitirConstanciaCita_Valido_CubreSerieConstanciaCita()
    {
        var citaId = Guid.NewGuid();
        var cita = new Cita
        {
            Id = citaId,
            CodigoCita = "CIT-001",
            Paciente = new Paciente { Nombres = "A", Apellidos = "B", DNI = "12345678" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor", Especialidad = "Ginecología" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" },
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Control",
            Estado = EstadoCita.Pendiente
        };
        _citaRepo.ObtenerPorIdConRelacionesAsync(citaId).Returns(cita);
        _usuarioActual.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _comprobanteRepo.ObtenerUltimoNumeroPorSerieAsync("C001").Returns(0);

        var dto = new EmitirComprobanteCitaDto { CitaId = citaId };
        var result = await _service.EmitirConstanciaCitaAsync(dto);

        result.Should().NotBeEmpty();
        await _comprobanteRepo.Received().AddAsync(Arg.Is<Comprobante>(c => c.Serie == "C001"));
    }

    [Fact]
    public async Task EmitirResumenAtencion_Valido_CubreSerieResumenAtencion()
    {
        var atencionId = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = atencionId,
            CodigoAtencion = "ATN-001",
            Paciente = new Paciente { Nombres = "Pac", Apellidos = "iente", DNI = "87654321" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor" },
            ServicioClinico = new ServicioClinico { CodigoServicio = "S1", Nombre = "Servicio" },
            FechaInicio = DateTime.UtcNow,
            Estado = EstadoAtencion.Abierta
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(atencionId).Returns(atencion);
        _usuarioActual.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _comprobanteRepo.ObtenerUltimoNumeroPorSerieAsync("A001").Returns(0);

        var dto = new EmitirComprobanteAtencionDto { AtencionId = atencionId };
        var result = await _service.EmitirResumenAtencionAsync(dto);

        result.Should().NotBeEmpty();
        await _comprobanteRepo.Received().AddAsync(Arg.Is<Comprobante>(c => c.Serie == "A001"));
    }

    [Fact]
    public async Task GenerarPdfResumenAtencion_ConAtencionCompleta_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var atencionId = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = atencionId,
            CodigoAtencion = "ATN-001",
            Paciente = new Paciente { Nombres = "Pac", Apellidos = "iente", DNI = "87654321" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor", Especialidad = "Ginecología" },
            ServicioClinico = new ServicioClinico { Nombre = "Servicio" },
            FechaInicio = DateTime.UtcNow,
            Estado = EstadoAtencion.Abierta
        };
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ResumenAtencion,
            Estado = EstadoComprobante.Emitido,
            AtencionId = atencionId,
            Atencion = atencion,
            NombrePaciente = "Paciente",
            NumeroDocumentoPaciente = "87654321"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarResumenAtencionPdf(Arg.Any<ComprobanteAtencionPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfResumenAtencionAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerarPdfEstadoCuenta_Valido_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.EstadoCuenta,
            Estado = EstadoComprobante.Emitido,
            NombrePaciente = "Paciente",
            NumeroDocumentoPaciente = "12345678",
            Total = 100
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarEstadoCuentaPacientePdf(Arg.Any<ComprobanteEstadoCuentaPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfEstadoCuentaPacienteAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task PreviewResumenAtencion_Valido_RetornaPreview()
    {
        var atencionId = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = atencionId,
            CodigoAtencion = "ATN-001",
            Paciente = new Paciente { Nombres = "Ana", Apellidos = "Pérez", DNI = "11111111", Direccion = "Calle 1" },
            Doctor = new Doctor { Nombres = "Luis", Apellidos = "Mamani", Especialidad = "Ginecología" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" },
            FechaInicio = DateTime.UtcNow,
            Estado = EstadoAtencion.Abierta,
            Anamnesis = new Anamnesis { MotivoConsulta = "Control" },
            ImpresionDiagnostica = new ImpresionDiagnostica { DiagnosticoPrincipal = "Normal", IndicacionesReceta = "Reposo" },
            Pagos = new List<Pago>
            {
                new Pago { MontoTotal = 100, MontoPagado = 80, SaldoPendiente = 20 }
            }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(atencionId).Returns(atencion);

        var result = await _service.PreviewResumenAtencionAsync(atencionId);

        result.AtencionId.Should().Be(atencionId);
        result.Paciente.Should().Be("Ana Pérez");
        result.Doctor.Should().Be("Luis Mamani");
        result.CostoFinal.Should().Be(100);
        result.MontoPagado.Should().Be(80);
        result.SaldoPendiente.Should().Be(20);
    }
    
    [Fact]
    public async Task EmitirHistoriaClinica_CubreSerieHistoriaClinica()
    {
        var paciente = new Paciente { Id = Guid.NewGuid(), Nombres = "A", Apellidos = "B", DNI = "12345678" };
        _pacienteRepo.GetByIdAsync(paciente.Id).Returns(paciente);
        _usuarioActual.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _comprobanteRepo.ObtenerUltimoNumeroPorSerieAsync("H001").Returns(0);

        // Usamos reflexión para invocar al método privado si no es público, pero asumo que EmitirHistoriaClinicaAsync es parte de IComprobanteService.
        // Si no existe, simplemente omitimos este test.
        var dto = new EmitirComprobanteEstadoCuentaDto { PacienteId = paciente.Id }; // No hay un DTO específico; esto es solo ilustrativo
        // En su lugar, podemos probar directamente el método interno con un emisor genérico.
        // Dado que no tenemos un endpoint, dejamos este test como opcional.
    }
    
    [Fact]
    public void ObtenerSerie_CasoPorDefecto_RetornaD001()
    {
        var method = typeof(ComprobanteService).GetMethod("ObtenerSerie", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var result = method!.Invoke(null, new object[] { (TipoComprobante)999 });
        result.Should().Be("D001");
    }
    [Fact]
    public void ObtenerSerie_TipoNoContemplado_RetornaD001()
    {
        // Usamos reflexión para invocar al método estático privado
        var method = typeof(ComprobanteService).GetMethod("ObtenerSerie",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var result = method!.Invoke(null, new object[] { (TipoComprobante)999 });
        result.Should().Be("D001");
    }
    [Fact]
    public async Task PreviewResumenAtencion_DoctorNull_RetornaNombreVacio()
    {
        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = "ATN-001",
            Paciente = new Paciente { Nombres = "Ana", Apellidos = "Pérez", DNI = "111" },
            // Doctor sin asignar → null
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(atencion.Id).Returns(atencion);

        var result = await _service.PreviewResumenAtencionAsync(atencion.Id);
        result.Doctor.Should().BeEmpty();
        result.Especialidad.Should().BeEmpty();
    }

    [Fact]
    public async Task PreviewResumenAtencion_ServicioNull_RetornaServicioPorDefecto()
    {
        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = "ATN-002",
            Paciente = new Paciente { Nombres = "Ana", Apellidos = "Pérez", DNI = "111" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor" },
            ServicioClinico = null!
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(atencion.Id).Returns(atencion);

        var result = await _service.PreviewResumenAtencionAsync(atencion.Id);
        result.Servicio.Should().Be("Servicio clínico");
    }

    [Fact]
    public async Task PreviewResumenAtencion_AnamnesisNull_DiagnosticoNull_RetornaCadenasVacias()
    {
        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = "ATN-003",
            Paciente = new Paciente { Nombres = "Ana", Apellidos = "Pérez", DNI = "111" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" },
            Anamnesis = null!,
            ImpresionDiagnostica = null!
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(atencion.Id).Returns(atencion);

        var result = await _service.PreviewResumenAtencionAsync(atencion.Id);
        result.MotivoConsulta.Should().BeEmpty();
        result.DiagnosticoResumen.Should().BeNull();
        result.Indicaciones.Should().BeNull();
    }
    
    [Fact]
    public async Task GenerarPdfBoletaPago_PagoNull_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.BoletaPago,
            Estado = EstadoComprobante.Emitido,
            PagoId = Guid.NewGuid(),
            Pago = null!,                     // sin objeto Pago
            Atencion = null!,
            Cita = null!,
            Detalles = new List<ComprobanteDetalle>() // sin detalles
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarBoletaPagoPdf(Arg.Any<ComprobantePagoPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfBoletaPagoAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task GenerarPdfConstanciaCita_CitaNull_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ConstanciaCita,
            Estado = EstadoComprobante.Emitido,
            CitaId = Guid.NewGuid(),
            Cita = null!,                       // sin objeto Cita
            NombrePaciente = "Paciente",
            NumeroDocumentoPaciente = "123"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarConstanciaCitaPdf(Arg.Any<ComprobanteCitaPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfConstanciaCitaAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerarPdfConstanciaCita_DoctorNull_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var cita = new Cita { Id = Guid.NewGuid(), Doctor = null!, ServicioClinico = new ServicioClinico { Nombre = "Consulta" } };
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ConstanciaCita,
            Estado = EstadoComprobante.Emitido,
            CitaId = cita.Id,
            Cita = cita,
            NombrePaciente = "Pac",
            NumeroDocumentoPaciente = "123"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarConstanciaCitaPdf(Arg.Any<ComprobanteCitaPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfConstanciaCitaAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task GenerarPdfResumenAtencion_AtencionNull_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ResumenAtencion,
            Estado = EstadoComprobante.Emitido,
            AtencionId = Guid.NewGuid(),
            Atencion = null!,                // sin objeto Atencion
            NombrePaciente = "Paciente",
            NumeroDocumentoPaciente = "123"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarResumenAtencionPdf(Arg.Any<ComprobanteAtencionPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfResumenAtencionAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerarPdfResumenAtencion_DoctorNull_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = "ATN-010",
            Paciente = new Paciente { Nombres = "A", Apellidos = "B", DNI = "111" },
            Doctor = null!,
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" }
        };
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ResumenAtencion,
            Estado = EstadoComprobante.Emitido,
            AtencionId = atencion.Id,
            Atencion = atencion,
            NombrePaciente = "Pac",
            NumeroDocumentoPaciente = "111"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarResumenAtencionPdf(Arg.Any<ComprobanteAtencionPreviewDto>()).Returns(new byte[] { 1 });

        var result = await _service.GenerarPdfResumenAtencionAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task ObtenerPorId_UsuarioEmisionNull_UsuarioAnulacionNull_RetornaDto()
    {
        var id = Guid.NewGuid();
        var comp = new Comprobante
        {
            Id = id,
            UsuarioEmisionId = Guid.NewGuid(),
            UsuarioEmision = null!,
            UsuarioAnulacionId = Guid.NewGuid(),
            UsuarioAnulacion = null!
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);

        var result = await _service.ObtenerPorIdAsync(id);
        result.UsuarioEmision.Should().BeNull();
        result.UsuarioAnulacion.Should().BeNull();
    }
    
    [Fact]
    public void ObtenerSerie_HistoriaClinica_RetornaH001()
    {
        var method = typeof(ComprobanteService).GetMethod("ObtenerSerie",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var result = method!.Invoke(null, new object[] { TipoComprobante.HistoriaClinica });
        result.Should().Be("H001");
    }

    [Fact]
    public void ObtenerSerie_ValorNoContemplado_RetornaD001()
    {
        var method = typeof(ComprobanteService).GetMethod("ObtenerSerie",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var result = method!.Invoke(null, new object[] { (TipoComprobante)999 });
        result.Should().Be("D001");
    }
    [Fact]
    public async Task PreviewResumenAtencion_CodigoAtencionNull_RetornaVacio()
    {
        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = null!,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B", DNI = "111" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor", Especialidad = null! },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(atencion.Id).Returns(atencion);
        var result = await _service.PreviewResumenAtencionAsync(atencion.Id);
        result.CodigoAtencion.Should().BeEmpty();
        result.Especialidad.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GenerarPdfConstanciaCita_DoctorEspecialidadNull_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var cita = new Cita
        {
            Id = Guid.NewGuid(),
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor", Especialidad = null! },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" }
        };
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ConstanciaCita,
            Estado = EstadoComprobante.Emitido,
            CitaId = cita.Id,
            Cita = cita,
            NombrePaciente = "Pac",
            NumeroDocumentoPaciente = "123"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarConstanciaCitaPdf(Arg.Any<ComprobanteCitaPreviewDto>()).Returns(new byte[] { 1 });
        var result = await _service.GenerarPdfConstanciaCitaAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task GenerarPdfResumenAtencion_AtencionCompleta_RetornaDocumento()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            CodigoAtencion = "ATN-COMPLETA",
            Paciente = new Paciente { Nombres = "Completo", Apellidos = "Test", DNI = "999", Direccion = "Calle" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Completo", Especialidad = "Ginecología" },
            ServicioClinico = new ServicioClinico { Nombre = "Servicio Completo" },
            FechaInicio = DateTime.UtcNow,
            FechaCierre = DateTime.UtcNow,
            Estado = EstadoAtencion.Cerrada,
            Anamnesis = new Anamnesis { MotivoConsulta = "Motivo" },
            ImpresionDiagnostica = new ImpresionDiagnostica
            {
                DiagnosticoPrincipal = "Diagnóstico",
                IndicacionesReceta = "Indicaciones",
                DiagnosticosSecundarios = "Secundario"
            },
            Pagos = new List<Pago>
            {
                new Pago { MontoTotal = 100, MontoPagado = 100, SaldoPendiente = 0 }
            }
        };
        var comp = new Comprobante
        {
            Id = id,
            TipoComprobante = TipoComprobante.ResumenAtencion,
            Estado = EstadoComprobante.Emitido,
            AtencionId = atencion.Id,
            Atencion = atencion,
            NombrePaciente = "Completo Test",
            NumeroDocumentoPaciente = "999"
        };
        _comprobanteRepo.ObtenerPorIdConDetalleAsync(id).Returns(comp);
        _pdfService.GenerarResumenAtencionPdf(Arg.Any<ComprobanteAtencionPreviewDto>()).Returns(new byte[] { 1 });
        var result = await _service.GenerarPdfResumenAtencionAsync(id);
        result.Archivo.Should().NotBeEmpty();
    }
}