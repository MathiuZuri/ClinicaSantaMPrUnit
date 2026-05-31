using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Pagos;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.IntegrationTests.PruebasInt.Pagos;

[Collection("IntegrationTests")]
public class PagosEndpointsTests : IntegrationTestBase
{
    public PagosEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_PagosPorPaciente_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync($"/api/pagos/paciente/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_PagosPorCita_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync($"/api/pagos/cita/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_PagosPorAtencion_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync($"/api/pagos/atencion/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Pagos_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        var dto = new RegistrarPagoDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            MontoTotal = 100,
            MontoPagado = 100,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Pago sin token"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_PagosPorPaciente_SinPagos_DeberiaRetornarListaVacia()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123456"
        );

        // Act
        var response = await Client.GetAsync($"/api/pagos/paciente/{paciente.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var pagos = await response.ReadDataAsJsonAsync<List<PagoResponseDto>>();

        pagos.Should().NotBeNull();
        pagos.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_PagosPorPaciente_ConPagos_DeberiaRetornarPagos()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123457"
        );

        var servicio = await db.ServiciosClinicos.FirstAsync();

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: paciente.Id,
            servicioClinicoId: servicio.Id,
            montoTotal: 120,
            montoPagado: 120,
            metodoPago: MetodoPago.Yape
        );

        // Act
        var response = await Client.GetAsync($"/api/pagos/paciente/{paciente.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var pagos = await response.ReadDataAsJsonAsync<List<PagoResponseDto>>();

        pagos.Should().NotBeNull();
        pagos.Should().ContainSingle();

        var pagoResponse = pagos!.Single();

        pagoResponse.Id.Should().Be(pago.Id);
        pagoResponse.PacienteId.Should().Be(paciente.Id);
        pagoResponse.ServicioClinicoId.Should().Be(servicio.Id);
        pagoResponse.MontoTotal.Should().Be(120);
        pagoResponse.MontoPagado.Should().Be(120);
        pagoResponse.SaldoPendiente.Should().Be(0);
        pagoResponse.Estado.Should().Be(EstadoPago.Pagado);
        pagoResponse.MetodoPago.Should().Be(MetodoPago.Yape);
        pagoResponse.PacienteNombre.Should().Contain(paciente.Nombres);
        pagoResponse.ServicioNombre.Should().Be(servicio.Nombre);
    }

    [Fact]
    public async Task Get_PagosPorCita_ConPago_DeberiaRetornarPagosDeLaCita()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var cita = await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id
        );

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            citaId: cita.Id,
            montoTotal: 70,
            montoPagado: 70,
            metodoPago: MetodoPago.Efectivo
        );

        // Act
        var response = await Client.GetAsync($"/api/pagos/cita/{cita.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagos = await response.ReadDataAsJsonAsync<List<PagoResponseDto>>();

        pagos.Should().NotBeNull();
        pagos.Should().ContainSingle();

        var pagoResponse = pagos!.Single();

        pagoResponse.Id.Should().Be(pago.Id);
        pagoResponse.CitaId.Should().Be(cita.Id);
        pagoResponse.Estado.Should().Be(EstadoPago.Pagado);
    }

    [Fact]
    public async Task Get_PagosPorAtencion_ConPago_DeberiaRetornarPagosDeLaAtencion()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(
            db,
            baseCita.Paciente.Id
        );

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 100,
            montoPagado: 0
        );

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 100,
            montoPagado: 50,
            metodoPago: MetodoPago.Transferencia
        );

        // Act
        var response = await Client.GetAsync($"/api/pagos/atencion/{atencion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagos = await response.ReadDataAsJsonAsync<List<PagoResponseDto>>();

        pagos.Should().NotBeNull();
        pagos.Should().ContainSingle();

        var pagoResponse = pagos!.Single();

        pagoResponse.Id.Should().Be(pago.Id);
        pagoResponse.AtencionId.Should().Be(atencion.Id);
        pagoResponse.Estado.Should().Be(EstadoPago.Parcial);
        pagoResponse.MontoPagado.Should().Be(50);
        pagoResponse.SaldoPendiente.Should().Be(50);
    }

    [Fact]
    public async Task Post_Pagos_TotalPagado_DeberiaRegistrarPagoPagado()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123458"
        );

        var servicio = await db.ServiciosClinicos.FirstAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = paciente.Id,
            ServicioClinicoId = servicio.Id,
            MontoTotal = 100,
            MontoPagado = 100,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Pago total de prueba"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var data = await JsonTestHelper.ReadDataAsync(response);

        data.TryGetProperty("id", out var idProperty)
            .Should()
            .BeTrue("la respuesta de registro debe devolver el id del pago");

        var pagoId = idProperty.GetGuid();

        var pago = await db.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pagoId);

        pago.Should().NotBeNull();
        pago!.PacienteId.Should().Be(paciente.Id);
        pago.ServicioClinicoId.Should().Be(servicio.Id);
        pago.MontoTotal.Should().Be(100);
        pago.MontoPagado.Should().Be(100);
        pago.SaldoPendiente.Should().Be(0);
        pago.Estado.Should().Be(EstadoPago.Pagado);
        pago.MetodoPago.Should().Be(MetodoPago.Efectivo);
        pago.CodigoPago.Should().NotBeNullOrWhiteSpace();
        pago.UsuarioRegistroId.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_Pagos_Parcial_DeberiaRegistrarPagoParcial()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123459"
        );

        var servicio = await db.ServiciosClinicos.FirstAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = paciente.Id,
            ServicioClinicoId = servicio.Id,
            MontoTotal = 100,
            MontoPagado = 40,
            MontoAdelanto = 10,
            MetodoPago = MetodoPago.Yape,
            Observacion = "Pago parcial de prueba"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await JsonTestHelper.ReadDataAsync(response);
        var pagoId = data.GetProperty("id").GetGuid();

        var pago = await db.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pagoId);

        pago.Should().NotBeNull();
        pago!.MontoTotal.Should().Be(100);
        pago.MontoPagado.Should().Be(40);
        pago.SaldoPendiente.Should().Be(60);
        pago.MontoAdelanto.Should().Be(10);
        pago.Estado.Should().Be(EstadoPago.Parcial);
        pago.MetodoPago.Should().Be(MetodoPago.Yape);
    }

    [Fact]
    public async Task Post_Pagos_ConMontoPagadoMayorAlTotal_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123460"
        );

        var servicio = await db.ServiciosClinicos.FirstAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = paciente.Id,
            ServicioClinicoId = servicio.Id,
            MontoTotal = 100,
            MontoPagado = 150,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Monto inválido"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Pagos_ConMontoAdelantoMayorAlTotal_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123461"
        );

        var servicio = await db.ServiciosClinicos.FirstAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = paciente.Id,
            ServicioClinicoId = servicio.Id,
            MontoTotal = 100,
            MontoPagado = 50,
            MontoAdelanto = 150,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Adelanto inválido"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Pagos_ConPacienteInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var servicio = await db.ServiciosClinicos.FirstAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = servicio.Id,
            MontoTotal = 100,
            MontoPagado = 100,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Paciente inexistente"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Pagos_ConServicioInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123462"
        );

        var dto = new RegistrarPagoDto
        {
            PacienteId = paciente.Id,
            ServicioClinicoId = Guid.NewGuid(),
            MontoTotal = 100,
            MontoPagado = 100,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Servicio inexistente"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Pagos_ConMontoTotalCero_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            MontoTotal = 0,
            MontoPagado = 0,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Monto cero"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pagos_ConMontoPagadoNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            MontoTotal = 100,
            MontoPagado = -1,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Monto negativo"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pagos_ConObservacionDemasiadoLarga_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            MontoTotal = 100,
            MontoPagado = 50,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = new string('A', 501)
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pagos_ConAtencionExistente_DeberiaActualizarMontosDeAtencion()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(
            db,
            baseCita.Paciente.Id
        );

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 100,
            montoPagado: 0
        );

        var dto = new RegistrarPagoDto
        {
            PacienteId = baseCita.Paciente.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            AtencionId = atencion.Id,
            MontoTotal = 100,
            MontoPagado = 60,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Tarjeta,
            Observacion = "Pago asociado a atención"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var atencionActualizada = await db.Atenciones
            .AsNoTracking()
            .FirstAsync(x => x.Id == atencion.Id);

        atencionActualizada.MontoPagado.Should().Be(60);
        atencionActualizada.SaldoPendiente.Should().Be(40);
    }

    [Fact]
    public async Task Post_Pagos_ConHistorialExistente_DeberiaCrearDetalleHistorial()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "70123463"
        );

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(
            db,
            paciente.Id
        );

        var servicio = await db.ServiciosClinicos.FirstAsync();

        var dto = new RegistrarPagoDto
        {
            PacienteId = paciente.Id,
            ServicioClinicoId = servicio.Id,
            MontoTotal = 100,
            MontoPagado = 100,
            MontoAdelanto = 0,
            MetodoPago = MetodoPago.Plin,
            Observacion = "Pago con historial"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await JsonTestHelper.ReadDataAsync(response);
        var pagoId = data.GetProperty("id").GetGuid();

        var detalle = await db.HistorialDetalles
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.HistorialClinicoId == historial.Id &&
                x.PagoId == pagoId &&
                x.TipoMovimiento == TipoMovimientoHistorial.PagoRegistrado);

        detalle.Should().NotBeNull();
        detalle!.Titulo.Should().Be("Pago registrado");
        detalle.Descripcion.Should().Contain("Se registró pago");
        detalle.UsuarioId.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Put_CambiarEstadoPago_Valido_DeberiaActualizarEstado()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "80123456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, pacienteId: paciente.Id, servicioClinicoId: servicio.Id, montoTotal: 100, montoPagado: 50);

        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Pagado };

        var putResponse = await Client.PutJsonAsync($"/api/pagos/{pago.Id}/estado", dto);
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/pagos/paciente/{paciente.Id}");
        var pagos = await getResponse.ReadDataAsJsonAsync<List<PagoResponseDto>>();
        var actualizado = pagos!.First(x => x.Id == pago.Id);
        actualizado.Estado.Should().Be(EstadoPago.Pagado);
    }

    [Fact]
    public async Task Put_CambiarEstadoPago_IdInexistente_DeberiaRetornarNotFound()
    {
        await LoginAsAdminAsync();

        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Anulado };

        var response = await Client.PutJsonAsync($"/api/pagos/{Guid.NewGuid()}/estado", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_CambiarEstadoPago_EliminadoConSaldo_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "80123457");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        // Crear un pago con saldo pendiente
        var pago = await TestDataSeeder.CrearPagoAsync(db, pacienteId: paciente.Id, servicioClinicoId: servicio.Id, montoTotal: 100, montoPagado: 50);

        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Eliminado };

        var response = await Client.PutJsonAsync($"/api/pagos/{pago.Id}/estado", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}