using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Pagos;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class PagoServiceTests
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IServicioClinicoRepository _servicioRepository;
    private readonly IAtencionRepository _atencionRepository;
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _detalleRepository;
    private readonly IUsuarioActualService _usuarioActualService;

    private readonly IPagoService _service;

    public PagoServiceTests()
    {
        _pagoRepository = Substitute.For<IPagoRepository>();
        _pacienteRepository = Substitute.For<IPacienteRepository>();
        _servicioRepository = Substitute.For<IServicioClinicoRepository>();
        _atencionRepository = Substitute.For<IAtencionRepository>();
        _historialRepository = Substitute.For<IHistorialClinicoRepository>();
        _detalleRepository = Substitute.For<IHistorialDetalleRepository>();
        _usuarioActualService = Substitute.For<IUsuarioActualService>();

        _service = new PagoService(
            _pagoRepository,
            _pacienteRepository,
            _servicioRepository,
            _atencionRepository,
            _historialRepository,
            _detalleRepository,
            _usuarioActualService);
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var pagos = new List<Pago> { CrearPagoEntidad(pacienteId: pacienteId) };

        _pagoRepository.ObtenerPorPacienteAsync(pacienteId).Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerPorPacienteAsync(pacienteId)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].PacienteId.Should().Be(pacienteId);
        resultado[0].PacienteNombre.Should().Be($"{pagos[0].Paciente.Nombres} {pagos[0].Paciente.Apellidos}");
        resultado[0].ServicioNombre.Should().Be(pagos[0].ServicioClinico.Nombre);
    }

    [Fact]
    public async Task ObtenerPorCitaAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var citaId = Guid.NewGuid();
        var pagos = new List<Pago> { CrearPagoEntidad(citaId: citaId) };

        _pagoRepository.ObtenerPorCitaAsync(citaId).Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerPorCitaAsync(citaId)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].CitaId.Should().Be(citaId);
    }

    [Fact]
    public async Task ObtenerPorAtencionAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var atencionId = Guid.NewGuid();
        var pagos = new List<Pago> { CrearPagoEntidad(atencionId: atencionId) };

        _pagoRepository.ObtenerPorAtencionAsync(atencionId).Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerPorAtencionAsync(atencionId)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].AtencionId.Should().Be(atencionId);
    }

    [Fact]
    public async Task RegistrarAsync_SiMontoPagadoEsMayorAlTotal_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.MontoPagado = 150m;
        dto.MontoTotal = 100m;

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _service.RegistrarAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El monto pagado no puede ser mayor al monto total.");
    }

    [Fact]
    public async Task RegistrarAsync_SiMontoAdelantoEsMayorAlTotal_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.MontoAdelanto = 120m;
        dto.MontoTotal = 100m;

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _service.RegistrarAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El monto de adelanto no puede ser mayor al monto total.");
    }

    [Fact]
    public async Task RegistrarAsync_SiPacienteNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = CrearDtoValido();

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns((Paciente?)null);

        // Act
        Func<Task> act = async () => await _service.RegistrarAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task RegistrarAsync_SiServicioNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var paciente = CrearPaciente(dto.PacienteId);

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns((ServicioClinico?)null);

        // Act
        Func<Task> act = async () => await _service.RegistrarAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Servicio no encontrado.");
    }

    [Fact]
    public async Task RegistrarAsync_SiNoHayAtencionNiHistorial_DebeRegistrarPagoYGuardar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();
        dto.AtencionId = null;

        var paciente = CrearPaciente(dto.PacienteId);
        var servicio = CrearServicio(dto.ServicioClinicoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);
        _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId).Returns((HistorialClinico?)null);

        // Act
        var resultado = await _service.RegistrarAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _pagoRepository.Received(1).AddAsync(Arg.Is<Pago>(p =>
            p.PacienteId == dto.PacienteId &&
            p.ServicioClinicoId == dto.ServicioClinicoId &&
            p.CitaId == dto.CitaId &&
            p.AtencionId == dto.AtencionId &&
            p.MontoTotal == dto.MontoTotal &&
            p.MontoPagado == dto.MontoPagado &&
            p.SaldoPendiente == dto.MontoTotal - dto.MontoPagado &&
            p.MontoAdelanto == dto.MontoAdelanto &&
            p.MetodoPago == dto.MetodoPago &&
            p.Estado == EstadoPago.Parcial &&
            p.Observacion == dto.Observacion &&
            p.UsuarioRegistroId == usuarioId &&
            !string.IsNullOrWhiteSpace(p.CodigoPago)));

        _atencionRepository.DidNotReceive().Update(Arg.Any<Atencion>());
        await _detalleRepository.DidNotReceive().AddAsync(Arg.Any<HistorialDetalle>());
        await _pagoRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RegistrarAsync_SiSaldoEsCero_DebeRegistrarPagoEnEstadoPagado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();
        dto.MontoTotal = 100m;
        dto.MontoPagado = 100m;
        dto.MontoAdelanto = 0m;
        dto.AtencionId = null;

        var paciente = CrearPaciente(dto.PacienteId);
        var servicio = CrearServicio(dto.ServicioClinicoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);
        _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId).Returns((HistorialClinico?)null);

        // Act
        await _service.RegistrarAsync(dto);

        // Assert
        await _pagoRepository.Received(1).AddAsync(Arg.Is<Pago>(p =>
            p.SaldoPendiente == 0m &&
            p.Estado == EstadoPago.Pagado));
    }

    [Fact]
    public async Task RegistrarAsync_SiExisteHistorial_DebeAgregarDetalle()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();

        var paciente = CrearPaciente(dto.PacienteId);
        var servicio = CrearServicio(dto.ServicioClinicoId);
        var historial = new HistorialClinico
        {
            Id = Guid.NewGuid(),
            PacienteId = dto.PacienteId
        };

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);
        _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId).Returns(historial);

        // Act
        var resultado = await _service.RegistrarAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        var descripcionEsperada = $"Se registró pago de S/ {dto.MontoPagado} por {servicio.Nombre}. Método: {dto.MetodoPago}.";

        await _detalleRepository.Received(1).AddAsync(Arg.Is<HistorialDetalle>(d =>
            d.HistorialClinicoId == historial.Id &&
            d.PagoId != null &&
            d.TipoMovimiento == TipoMovimientoHistorial.PagoRegistrado &&
            d.Titulo == "Pago registrado" &&
            d.Descripcion == descripcionEsperada &&
            d.UsuarioId == usuarioId &&
            !string.IsNullOrWhiteSpace(d.CodigoDetalle)));
    }

    private static RegistrarPagoDto CrearDtoValido()
    {
        return new RegistrarPagoDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            CitaId = Guid.NewGuid(),
            AtencionId = null,
            MontoTotal = 100m,
            MontoPagado = 60m,
            MontoAdelanto = 20m,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Pago inicial"
        };
    }

    private static Pago CrearPagoEntidad(
        Guid? pacienteId = null,
        Guid? citaId = null,
        Guid? atencionId = null)
    {
        var pId = pacienteId ?? Guid.NewGuid();

        return new Pago
        {
            Id = Guid.NewGuid(),
            CodigoPago = "ABCDE-PAG-2026-12345678",
            PacienteId = pId,
            Paciente = CrearPaciente(pId),
            ServicioClinicoId = Guid.NewGuid(),
            ServicioClinico = CrearServicio(Guid.NewGuid()),
            CitaId = citaId,
            AtencionId = atencionId,
            MontoTotal = 100m,
            MontoPagado = 60m,
            SaldoPendiente = 40m,
            MontoAdelanto = 20m,
            MetodoPago = MetodoPago.Yape,
            Estado = EstadoPago.Parcial,
            Observacion = "Pago parcial",
            FechaPago = DateTime.UtcNow
        };
    }

    private static Paciente CrearPaciente(Guid id)
    {
        return new Paciente
        {
            Id = id,
            DNI = "12345678",
            Nombres = "Ana",
            Apellidos = "Quispe"
        };
    }

    private static ServicioClinico CrearServicio(Guid id)
    {
        return new ServicioClinico
        {
            Id = id,
            CodigoServicio = "ATEGEN",
            Nombre = "Atención general"
        };
    }
    
    [Fact]
    public async Task CambiarEstadoAsync_SiPagoNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Pagado };
        _pagoRepository.GetByIdAsync(id).Returns((Pago?)null);

        Func<Task> act = async () => await _service.CambiarEstadoAsync(id, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Pago no encontrado.");
    }

    [Fact]
    public async Task CambiarEstadoAsync_SiPagoEliminado_LanzaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var pago = new Pago { Id = id, Estado = EstadoPago.Eliminado };
        _pagoRepository.GetByIdAsync(id).Returns(pago);

        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Pagado };

        Func<Task> act = async () => await _service.CambiarEstadoAsync(id, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede modificar un pago eliminado.");
    }

    [Fact]
    public async Task CambiarEstadoAsync_SiPasarAEliminadoConSaldoPendiente_LanzaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var pago = new Pago { Id = id, Estado = EstadoPago.Parcial, SaldoPendiente = 50 };
        _pagoRepository.GetByIdAsync(id).Returns(pago);

        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Eliminado };

        Func<Task> act = async () => await _service.CambiarEstadoAsync(id, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede eliminar un pago con saldo pendiente. Primero regularice la deuda.");
    }

    [Fact]
    public async Task CambiarEstadoAsync_CambioValido_DebeActualizarYGuardar()
    {
        var id = Guid.NewGuid();
        var pago = new Pago { Id = id, Estado = EstadoPago.Parcial, SaldoPendiente = 0 };
        _pagoRepository.GetByIdAsync(id).Returns(pago);

        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Pagado };

        await _service.CambiarEstadoAsync(id, dto);

        pago.Estado.Should().Be(EstadoPago.Pagado);
        _pagoRepository.Received(1).Update(pago);
        await _pagoRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task ObtenerPorPacienteAsync_CuandoPacienteEsNull_DebeRetornarNombreVacio()
    {
        var pacienteId = Guid.NewGuid();
        var pago = CrearPagoEntidad(pacienteId: pacienteId);
        pago.Paciente = null;

        _pagoRepository.ObtenerPorPacienteAsync(pacienteId).Returns(new List<Pago> { pago });

        var resultado = (await _service.ObtenerPorPacienteAsync(pacienteId)).ToList();
        resultado[0].PacienteNombre.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_CuandoServicioClinicoEsNull_DebeRetornarNombreVacio()
    {
        var pacienteId = Guid.NewGuid();
        var pago = CrearPagoEntidad(pacienteId: pacienteId);
        pago.ServicioClinico = null;

        _pagoRepository.ObtenerPorPacienteAsync(pacienteId).Returns(new List<Pago> { pago });

        var resultado = (await _service.ObtenerPorPacienteAsync(pacienteId)).ToList();
        resultado[0].ServicioNombre.Should().BeEmpty();
    }
    
    [Fact]
    public async Task RegistrarAsync_ConAtencionIdPeroAtencionNoExiste_NoActualizaAtencion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();
        dto.AtencionId = Guid.NewGuid(); // tiene valor

        var paciente = CrearPaciente(dto.PacienteId);
        var servicio = CrearServicio(dto.ServicioClinicoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);
        _atencionRepository.GetByIdAsync(dto.AtencionId.Value).Returns((Atencion?)null); // no se encuentra
        _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId).Returns((HistorialClinico?)null);

        // Act
        var resultado = await _service.RegistrarAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();
        _atencionRepository.DidNotReceive().Update(Arg.Any<Atencion>());
        await _pagoRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task RegistrarAsync_CuandoMontoPagadoEsCero_DebeCrearPagoEnEstadoPendiente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();
        dto.MontoTotal = 100m;
        dto.MontoPagado = 0m;        // no se ha pagado nada
        dto.MontoAdelanto = 0m;
        dto.AtencionId = null;

        var paciente = CrearPaciente(dto.PacienteId);
        var servicio = CrearServicio(dto.ServicioClinicoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);
        _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId).Returns((HistorialClinico?)null);

        // Act
        await _service.RegistrarAsync(dto);

        // Assert
        await _pagoRepository.Received(1).AddAsync(Arg.Is<Pago>(p =>
            p.SaldoPendiente == 100m &&
            p.Estado == EstadoPago.Pendiente));
        await _pagoRepository.Received(1).SaveChangesAsync();
    }
    
    
}