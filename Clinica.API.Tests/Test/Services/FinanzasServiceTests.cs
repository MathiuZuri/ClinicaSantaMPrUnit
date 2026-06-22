using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Finanzas;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class FinanzasServiceTests
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IAjusteFinancieroRepository _ajusteRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    private readonly FinanzasService _service;

    public FinanzasServiceTests()
    {
        _pagoRepository = Substitute.For<IPagoRepository>();
        _pacienteRepository = Substitute.For<IPacienteRepository>();
        _ajusteRepository = Substitute.For<IAjusteFinancieroRepository>();
        _usuarioActualService = Substitute.For<IUsuarioActualService>();

        _service = new FinanzasService(
            _pagoRepository,
            _pacienteRepository,
            _ajusteRepository,
            _usuarioActualService);
    }

    [Fact]
    public async Task RegistrarAjusteFinancieroAsync_SiPagoIdEsVacio_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.Empty,
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 10m,
            Motivo = "Motivo válido"
        };

        // Act
        Func<Task> act = async () => await _service.RegistrarAjusteFinancieroAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El identificador del pago es obligatorio.");
    }

    [Fact]
    public async Task RegistrarAjusteFinancieroAsync_SiMontoNoEsValido_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 0,
            Motivo = "Motivo válido"
        };

        // Act
        Func<Task> act = async () => await _service.RegistrarAjusteFinancieroAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El monto del ajuste debe ser mayor a 0.");
    }

    [Fact]
    public async Task RegistrarAjusteFinancieroAsync_SiMotivoEsVacio_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 10m,
            Motivo = "   "
        };

        // Act
        Func<Task> act = async () => await _service.RegistrarAjusteFinancieroAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El motivo del ajuste financiero es obligatorio.");
    }

    [Fact]
    public async Task RegistrarAjusteFinancieroAsync_SiPagoNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 10m,
            Motivo = "Motivo válido"
        };

        _pagoRepository.GetByIdAsync(dto.PagoId).Returns((Pago?)null);

        // Act
        Func<Task> act = async () => await _service.RegistrarAjusteFinancieroAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Pago no encontrado.");
    }

    [Fact]
    public async Task RegistrarAjusteFinancieroAsync_SiExisteDuplicado_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 10m,
            Motivo = "Motivo válido"
        };

        var pago = CrearPago(dto.PagoId);

        _pagoRepository.GetByIdAsync(dto.PagoId).Returns(pago);
        _ajusteRepository.ExisteAjusteSimilarAsync(dto.PagoId, dto.TipoAjuste, dto.MontoAjuste, dto.Motivo)
            .Returns(true);

        // Act
        Func<Task> act = async () => await _service.RegistrarAjusteFinancieroAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe un ajuste financiero similar registrado para este pago.");
    }

    [Fact]
    public async Task RegistrarAjusteFinancieroAsync_SiTodoEsValido_DebeGuardarAjuste()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Recargo,
            MontoAjuste = 15m,
            Motivo = "  Servicio adicional  ",
            Observacion = "  Cobro manual  "
        };

        var pago = CrearPago(dto.PagoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pagoRepository.GetByIdAsync(dto.PagoId).Returns(pago);
        _ajusteRepository.ExisteAjusteSimilarAsync(dto.PagoId, dto.TipoAjuste, dto.MontoAjuste, dto.Motivo)
            .Returns(false);

        // Act
        var resultado = await _service.RegistrarAjusteFinancieroAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _ajusteRepository.Received(1).AddAsync(Arg.Is<AjusteFinanciero>(a =>
            a.PagoId == pago.Id &&
            a.AtencionId == pago.AtencionId &&
            a.PacienteId == pago.PacienteId &&
            a.TipoAjuste == dto.TipoAjuste &&
            a.MontoAjuste == dto.MontoAjuste &&
            a.Motivo == "Servicio adicional" &&
            a.Observacion == "Cobro manual" &&
            a.UsuarioRegistroId == usuarioId));

        await _ajusteRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ObtenerLibroDiarioAsync_DebeRetornarSoloPagosValidosDelDia()
    {
        // Arrange
        var fecha = new DateOnly(2026, 1, 10);

        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 10, 0, 0), estado: EstadoPago.Pagado),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 12, 0, 0), estado: EstadoPago.Parcial),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 14, 0, 0), estado: EstadoPago.Anulado),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 11, 9, 0, 0), estado: EstadoPago.Pagado)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerLibroDiarioAsync(fecha)).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(x => DateOnly.FromDateTime(x.FechaPago) == fecha);
        resultado.Should().OnlyContain(x => x.EstadoPago != EstadoPago.Anulado.ToString());
    }

    [Fact]
    public async Task ObtenerResumenDiarioAsync_DebeCalcularTotalesCorrectamente()
    {
        // Arrange
        var fecha = new DateOnly(2026, 1, 10);

        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 10, 0, 0), montoTotal: 100m, montoPagado: 100m, saldo: 0m, estado: EstadoPago.Pagado),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 11, 0, 0), montoTotal: 120m, montoPagado: 60m, saldo: 60m, estado: EstadoPago.Parcial),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 12, 0, 0), montoTotal: 90m, montoPagado: 0m, saldo: 90m, estado: EstadoPago.Pendiente),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 13, 0, 0), montoTotal: 50m, montoPagado: 50m, saldo: 0m, estado: EstadoPago.Reembolsado)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = await _service.ObtenerResumenDiarioAsync(fecha);

        // Assert
        resultado.Fecha.Should().Be(fecha);
        resultado.TotalIngresos.Should().Be(160m);
        resultado.TotalPendiente.Should().Be(150m);
        resultado.TotalDeuda.Should().Be(150m);
        resultado.CantidadPagos.Should().Be(3);
        resultado.PagosCompletados.Should().Be(1);
        resultado.PagosParciales.Should().Be(1);
        resultado.PagosPendientes.Should().Be(2);
        resultado.Pagos.Should().HaveCount(3);
    }

    [Fact]
    public async Task ObtenerResumenMensualAsync_SiMesEsInvalido_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerResumenMensualAsync(2026, 13);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El mes ingresado no es válido.");
    }

    [Fact]
    public async Task ObtenerResumenMensualAsync_DebeAgruparPorDias()
    {
        // Arrange
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 2, 1, 10, 0, 0)),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 2, 1, 12, 0, 0)),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 2, 2, 9, 0, 0))
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = await _service.ObtenerResumenMensualAsync(2026, 2);

        // Assert
        resultado.Anio.Should().Be(2026);
        resultado.Mes.Should().Be(2);
        resultado.CantidadPagos.Should().Be(3);
        resultado.Dias.Should().HaveCount(2);
        resultado.Dias.Should().Contain(x => x.Fecha == new DateOnly(2026, 2, 1));
        resultado.Dias.Should().Contain(x => x.Fecha == new DateOnly(2026, 2, 2));
    }

    [Fact]
    public async Task ObtenerResumenAnualAsync_SiAnioEsInvalido_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerResumenAnualAsync(1999);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El año ingresado no es válido.");
    }

    [Fact]
    public async Task ObtenerResumenAnualAsync_DebeRetornarDoceMeses()
    {
        // Arrange
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 10, 0, 0)),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 5, 15, 11, 0, 0)),
            CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 12, 20, 12, 0, 0))
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = await _service.ObtenerResumenAnualAsync(2026);

        // Assert
        resultado.Anio.Should().Be(2026);
        resultado.Meses.Should().HaveCount(12);
        resultado.CantidadPagos.Should().Be(3);
    }

    [Fact]
    public async Task ObtenerPagosPendientesAsync_DebeRetornarSoloPendientesOConSaldo()
    {
        // Arrange
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Pendiente, saldo: 40m),
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Parcial, saldo: 10m),
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Pagado, saldo: 0m),
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Anulado, saldo: 50m)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerPagosPendientesAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObtenerPagosPagadosAsync_DebeRetornarSoloPagadosSinSaldo()
    {
        // Arrange
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Pagado, saldo: 0m),
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Pagado, saldo: 5m),
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Parcial, saldo: 10m)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerPagosPagadosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].EstadoPago.Should().Be(EstadoPago.Pagado.ToString());
    }

    [Fact]
    public async Task ObtenerPagosParcialesAsync_DebeRetornarSoloParciales()
    {
        // Arrange
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Parcial),
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Pendiente),
            CrearPago(Guid.NewGuid(), estado: EstadoPago.Pagado)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerPagosParcialesAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].EstadoPago.Should().Be(EstadoPago.Parcial.ToString());
    }

    [Fact]
    public async Task ObtenerPagoPorCodigoAsync_SiCodigoEsVacio_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerPagoPorCodigoAsync("   ");

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El código de pago es obligatorio.");
    }

    [Fact]
    public async Task ObtenerPagoPorCodigoAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        _pagoRepository.ObtenerPorCodigoConDetalleAsync("CODIGO").Returns((Pago?)null);

        // Act
        var resultado = await _service.ObtenerPagoPorCodigoAsync("CODIGO");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPagoPorCodigoAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var pago = CrearPago(Guid.NewGuid());
        _pagoRepository.ObtenerPorCodigoConDetalleAsync("CODIGO").Returns(pago);

        // Act
        var resultado = await _service.ObtenerPagoPorCodigoAsync("CODIGO");

        // Assert
        resultado.Should().NotBeNull();
        resultado!.CodigoPago.Should().Be(pago.CodigoPago);
    }

    [Fact]
    public async Task ObtenerEstadoCuentaPacienteAsync_SiPacienteIdEsVacio_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerEstadoCuentaPacienteAsync(Guid.Empty);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El identificador del paciente es obligatorio.");
    }

    [Fact]
    public async Task ObtenerEstadoCuentaPacienteAsync_SiPacienteNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        _pacienteRepository.GetByIdAsync(pacienteId).Returns((Paciente?)null);

        // Act
        Func<Task> act = async () => await _service.ObtenerEstadoCuentaPacienteAsync(pacienteId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task ObtenerEstadoCuentaPacienteAsync_DebeCalcularEstadoAgrupadoPorAtencion()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var atencionId = Guid.NewGuid();
        var paciente = CrearPaciente(pacienteId);

        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), pacienteId: pacienteId, atencionId: atencionId, montoTotal: 100m, montoPagado: 40m, saldo: 60m, estado: EstadoPago.Parcial),
            CrearPago(Guid.NewGuid(), pacienteId: pacienteId, atencionId: atencionId, montoTotal: 100m, montoPagado: 60m, saldo: 40m, estado: EstadoPago.Parcial)
        };

        _pacienteRepository.GetByIdAsync(pacienteId).Returns(paciente);
        _pagoRepository.ObtenerPorPacienteAsync(pacienteId).Returns(pagos);

        // Act
        var resultado = await _service.ObtenerEstadoCuentaPacienteAsync(pacienteId);

        // Assert
        resultado.PacienteId.Should().Be(pacienteId);
        resultado.TotalFacturado.Should().Be(100m);
        resultado.TotalPagado.Should().Be(100m);
        resultado.TotalPendiente.Should().Be(0m);
        resultado.CantidadPagos.Should().Be(2);
        resultado.PagosCompletados.Should().Be(1);
    }

    [Fact]
    public async Task ObtenerDeudasRealesAsync_DebeRetornarSoloAtencionesConDeuda()
    {
        // Arrange
        var atencionConDeuda = Guid.NewGuid();
        var atencionPagada = Guid.NewGuid();

        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), atencionId: atencionConDeuda, montoTotal: 100m, montoPagado: 40m, saldo: 60m, estado: EstadoPago.Parcial),
            CrearPago(Guid.NewGuid(), atencionId: atencionPagada, montoTotal: 80m, montoPagado: 80m, saldo: 0m, estado: EstadoPago.Pagado)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = (await _service.ObtenerDeudasRealesAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].AtencionId.Should().Be(atencionConDeuda);
        resultado[0].TieneDeuda.Should().BeTrue();
        resultado[0].DeudaTotal.Should().Be(resultado[0].SaldoReal);
    }

    [Fact]
    public async Task ObtenerDeudasRealesPacienteAsync_SiPacienteIdEsVacio_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerDeudasRealesPacienteAsync(Guid.Empty);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El identificador del paciente es obligatorio.");
    }

    [Fact]
    public async Task ObtenerDeudasRealesPacienteAsync_SiPacienteNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        _pacienteRepository.GetByIdAsync(pacienteId).Returns((Paciente?)null);

        // Act
        Func<Task> act = async () => await _service.ObtenerDeudasRealesPacienteAsync(pacienteId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task ObtenerEstadoPagoAtencionAsync_SiAtencionIdEsVacio_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerEstadoPagoAtencionAsync(Guid.Empty);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El identificador de la atención es obligatorio.");
    }

    [Fact]
    public async Task ObtenerEstadoPagoAtencionAsync_SiNoHayPagos_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var atencionId = Guid.NewGuid();
        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(new List<Pago>());

        // Act
        Func<Task> act = async () => await _service.ObtenerEstadoPagoAtencionAsync(atencionId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("No se encontraron pagos asociados a la atención.");
    }

    [Fact]
    public async Task ObtenerEstadoPagoAtencionAsync_DebeCalcularSobrepago()
    {
        // Arrange
        var atencionId = Guid.NewGuid();

        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), atencionId: atencionId, montoTotal: 100m, montoPagado: 70m, saldo: 30m, estado: EstadoPago.Parcial),
            CrearPago(Guid.NewGuid(), atencionId: atencionId, montoTotal: 100m, montoPagado: 50m, saldo: 50m, estado: EstadoPago.Parcial)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = await _service.ObtenerEstadoPagoAtencionAsync(atencionId);

        // Assert
        resultado.AtencionId.Should().Be(atencionId);
        resultado.MontoTotal.Should().Be(100m);
        resultado.TotalPagado.Should().Be(120m);
        resultado.Sobrepago.Should().Be(20m);
        resultado.TieneSobrepago.Should().BeTrue();
        resultado.EstadoFinanciero.Should().Be("Sobrepagado");
    }

    [Fact]
    public async Task ObtenerResumenFinancieroMensualCompletoAsync_DebeRetornarCajaAtencionesYAjustes()
    {
        // Arrange
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), atencionId: Guid.NewGuid(), fechaPago: new DateTime(2026, 3, 10, 10, 0, 0), montoTotal: 100m, montoPagado: 100m, saldo: 0m, estado: EstadoPago.Pagado),
            CrearPago(Guid.NewGuid(), atencionId: Guid.NewGuid(), fechaPago: new DateTime(2026, 3, 15, 11, 0, 0), montoTotal: 120m, montoPagado: 60m, saldo: 60m, estado: EstadoPago.Parcial)
        };

        var ajustes = new List<AjusteFinanciero>
        {
            CrearAjuste(Guid.NewGuid(), new DateTime(2026, 3, 20, 9, 0, 0))
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);
        _ajusteRepository.ObtenerTodosConDetalleAsync().Returns(ajustes);

        // Act
        var resultado = await _service.ObtenerResumenFinancieroMensualCompletoAsync(2026, 3);

        // Assert
        resultado.Anio.Should().Be(2026);
        resultado.Mes.Should().Be(3);
        resultado.ResumenCaja.CantidadMovimientos.Should().Be(2);
        resultado.ResumenRealAtenciones.EstadosAtenciones.Should().HaveCount(2);
        resultado.AjustesFinancieros.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObtenerAjustesFinancierosAsync_DebeRetornarDtosOrdenados()
    {
        // Arrange
        var ajustes = new List<AjusteFinanciero>
        {
            CrearAjuste(Guid.NewGuid(), new DateTime(2026, 3, 10, 9, 0, 0)),
            CrearAjuste(Guid.NewGuid(), new DateTime(2026, 3, 20, 9, 0, 0))
        };

        _ajusteRepository.ObtenerTodosConDetalleAsync().Returns(ajustes);

        // Act
        var resultado = (await _service.ObtenerAjustesFinancierosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].FechaRegistro.Should().BeAfter(resultado[1].FechaRegistro);
    }

    [Fact]
    public async Task ObtenerAjustesPorAtencionAsync_SiAtencionIdEsVacio_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerAjustesPorAtencionAsync(Guid.Empty);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El identificador de la atención es obligatorio.");
    }

    [Fact]
    public async Task ObtenerAjustesPorPagoAsync_SiPagoIdEsVacio_DebeLanzarInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerAjustesPorPagoAsync(Guid.Empty);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El identificador del pago es obligatorio.");
    }

    private static Pago CrearPago(
        Guid id,
        Guid? pacienteId = null,
        Guid? atencionId = null,
        DateTime? fechaPago = null,
        decimal montoTotal = 100m,
        decimal montoPagado = 60m,
        decimal saldo = 40m,
        EstadoPago estado = EstadoPago.Parcial)
    {
        var pId = pacienteId ?? Guid.NewGuid();

        return new Pago
        {
            Id = id,
            CodigoPago = $"PAG-{id.ToString("N")[..5].ToUpper()}",
            PacienteId = pId,
            Paciente = new Paciente
            {
                Id = pId,
                Nombres = "Ana",
                Apellidos = "Quispe",
                DNI = "12345678"
            },
            ServicioClinicoId = Guid.NewGuid(),
            ServicioClinico = new ServicioClinico
            {
                Id = Guid.NewGuid(),
                Nombre = "Consulta obstétrica"
            },
            AtencionId = atencionId,
            MontoTotal = montoTotal,
            MontoPagado = montoPagado,
            SaldoPendiente = saldo,
            MontoAdelanto = 0m,
            MetodoPago = MetodoPago.Efectivo,
            Estado = estado,
            FechaPago = fechaPago ?? new DateTime(2026, 1, 10, 10, 0, 0),
            UsuarioRegistro = new Usuario
            {
                Nombres = "Carlos",
                Apellidos = "Mamani"
            }
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

    private static AjusteFinanciero CrearAjuste(Guid id, DateTime fecha)
    {
        return new AjusteFinanciero
        {
            Id = id,
            PagoId = Guid.NewGuid(),
            Pago = new Pago
            {
                CodigoPago = "PAG-ABCDE"
            },
            AtencionId = Guid.NewGuid(),
            PacienteId = Guid.NewGuid(),
            Paciente = new Paciente
            {
                Nombres = "Ana",
                Apellidos = "Quispe",
                DNI = "12345678"
            },
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 15m,
            Motivo = "Descuento autorizado",
            Observacion = "Observación",
            UsuarioRegistro = new Usuario
            {
                Nombres = "Carlos",
                Apellidos = "Mamani"
            },
            FechaRegistro = fecha
        };
    }
    
    [Fact]
    public async Task RegistrarAjusteFinancieroAsync_ConObservacionNull_DebeGuardarAjusteSinObservacion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Recargo,
            MontoAjuste = 15m,
            Motivo = "Motivo",
            Observacion = null // <-- null
        };

        var pago = CrearPago(dto.PagoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pagoRepository.GetByIdAsync(dto.PagoId).Returns(pago);
        _ajusteRepository.ExisteAjusteSimilarAsync(dto.PagoId, dto.TipoAjuste, dto.MontoAjuste, dto.Motivo).Returns(false);

        // Act
        var resultado = await _service.RegistrarAjusteFinancieroAsync(dto);

        // Assert
        await _ajusteRepository.Received(1).AddAsync(Arg.Is<AjusteFinanciero>(a =>
            a.Observacion == null)); // cubre la rama null del ?.
    }
    
    [Fact]
    public async Task ObtenerEstadoPagoAtencionAsync_CuandoPagadoCompleto_DebeRetornarEstadoPagado()
    {
        // Arrange
        var atencionId = Guid.NewGuid();
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), atencionId: atencionId, montoTotal: 100m, montoPagado: 100m, saldo: 0m, estado: EstadoPago.Pagado)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = await _service.ObtenerEstadoPagoAtencionAsync(atencionId);

        // Assert
        resultado.EstadoFinanciero.Should().Be("Pagado");
        resultado.TieneDeuda.Should().BeFalse();
        resultado.TieneSobrepago.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerEstadoPagoAtencionAsync_CuandoNoHayAbonos_DebeRetornarPendiente()
    {
        // Arrange
        var atencionId = Guid.NewGuid();
        var pagos = new List<Pago>
        {
            CrearPago(Guid.NewGuid(), atencionId: atencionId, montoTotal: 100m, montoPagado: 0m, saldo: 100m, estado: EstadoPago.Pendiente)
        };

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(pagos);

        // Act
        var resultado = await _service.ObtenerEstadoPagoAtencionAsync(atencionId);

        // Assert
        resultado.EstadoFinanciero.Should().Be("Pendiente");
        resultado.TieneDeuda.Should().BeTrue();
    }
    
    [Fact]
    public async Task ObtenerLibroDiarioAsync_CuandoPacienteEsNull_DebeMapearPacienteVacio()
    {
        // Arrange
        var fecha = new DateOnly(2026, 1, 10);
        var pago = CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 10, 0, 0), estado: EstadoPago.Pagado);
        pago.Paciente = null!;

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(new List<Pago> { pago });

        // Act
        var resultado = (await _service.ObtenerLibroDiarioAsync(fecha)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].Paciente.Should().BeEmpty();
        resultado[0].DniPaciente.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerLibroDiarioAsync_CuandoServicioClinicoEsNull_DebeMapearServicioVacio()
    {
        var fecha = new DateOnly(2026, 1, 10);
        var pago = CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 10, 0, 0), estado: EstadoPago.Pagado);
        pago.ServicioClinico = null!;

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(new List<Pago> { pago });

        var resultado = (await _service.ObtenerLibroDiarioAsync(fecha)).ToList();
        resultado[0].Servicio.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerLibroDiarioAsync_CuandoUsuarioRegistroEsNull_DebeMapearRegistradoPorVacio()
    {
        var fecha = new DateOnly(2026, 1, 10);
        var pago = CrearPago(Guid.NewGuid(), fechaPago: new DateTime(2026, 1, 10, 10, 0, 0), estado: EstadoPago.Pagado);
        pago.UsuarioRegistro = null!;

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(new List<Pago> { pago });

        var resultado = (await _service.ObtenerLibroDiarioAsync(fecha)).ToList();
        resultado[0].RegistradoPor.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ObtenerAjustesFinancierosAsync_CuandoPagoEsNull_DebeMapearCodigoPagoVacio()
    {
        var ajuste = CrearAjuste(Guid.NewGuid(), new DateTime(2026, 3, 10));
        ajuste.Pago = null!;

        _ajusteRepository.ObtenerTodosConDetalleAsync().Returns(new List<AjusteFinanciero> { ajuste });

        var resultado = (await _service.ObtenerAjustesFinancierosAsync()).ToList();
        resultado[0].CodigoPago.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerAjustesFinancierosAsync_CuandoPacienteEsNull_DebeMapearPacienteVacio()
    {
        var ajuste = CrearAjuste(Guid.NewGuid(), new DateTime(2026, 3, 10));
        ajuste.Paciente = null!;

        _ajusteRepository.ObtenerTodosConDetalleAsync().Returns(new List<AjusteFinanciero> { ajuste });

        var resultado = (await _service.ObtenerAjustesFinancierosAsync()).ToList();
        resultado[0].Paciente.Should().BeEmpty();
        resultado[0].DniPaciente.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerAjustesFinancierosAsync_CuandoUsuarioRegistroEsNull_DebeMapearRegistradoPorVacio()
    {
        var ajuste = CrearAjuste(Guid.NewGuid(), new DateTime(2026, 3, 10));
        ajuste.UsuarioRegistro = null!;

        _ajusteRepository.ObtenerTodosConDetalleAsync().Returns(new List<AjusteFinanciero> { ajuste });

        var resultado = (await _service.ObtenerAjustesFinancierosAsync()).ToList();
        resultado[0].RegistradoPor.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ObtenerEstadoPagoAtencionAsync_CuandoPacienteEsNull_DebeMapearPacienteVacio()
    {
        var atencionId = Guid.NewGuid();
        var pago = CrearPago(Guid.NewGuid(), atencionId: atencionId, montoTotal: 100m, montoPagado: 50m, saldo: 50m, estado: EstadoPago.Parcial);
        pago.Paciente = null!;

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(new List<Pago> { pago });

        var resultado = await _service.ObtenerEstadoPagoAtencionAsync(atencionId);
        resultado.Paciente.Should().BeEmpty();
        resultado.DniPaciente.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerEstadoPagoAtencionAsync_CuandoServicioClinicoEsNull_DebeMapearServicioVacio()
    {
        var atencionId = Guid.NewGuid();
        var pago = CrearPago(Guid.NewGuid(), atencionId: atencionId, montoTotal: 100m, montoPagado: 50m, saldo: 50m, estado: EstadoPago.Parcial);
        pago.ServicioClinico = null!;

        _pagoRepository.ObtenerTodosConDetalleAsync().Returns(new List<Pago> { pago });

        var resultado = await _service.ObtenerEstadoPagoAtencionAsync(atencionId);
        resultado.Servicio.Should().BeEmpty();
    }
}