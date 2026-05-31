using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Historiales;
using Clinica.Domain.Enums;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Historiales;

[Collection("IntegrationTests")]
public class HistorialesEndpointsTests : IntegrationTestBase
{
    public HistorialesEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_HistorialPorPaciente_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync($"/api/historiales/paciente/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_HistorialConDetalles_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync($"/api/historiales/{Guid.NewGuid()}/detalles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_HistorialPorPaciente_Existente_DeberiaRetornarHistorial()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseHistorial = await TestDataSeeder.CrearPacienteConHistorialAsync(
            db,
            dni: "81526374"
        );

        // Act
        var response = await Client.GetAsync($"/api/historiales/paciente/{baseHistorial.Paciente.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var historial = await response.ReadDataAsJsonAsync<HistorialClinicoResponseDto>();

        historial.Should().NotBeNull();
        historial!.Id.Should().Be(baseHistorial.Historial.Id);
        historial.PacienteId.Should().Be(baseHistorial.Paciente.Id);
        historial.PacienteDni.Should().Be(baseHistorial.Paciente.DNI);
        historial.PacienteNombre.Should().Contain(baseHistorial.Paciente.Nombres);
        historial.PacienteNombre.Should().Contain(baseHistorial.Paciente.Apellidos);
        historial.CodigoHistorial.Should().NotBeNullOrWhiteSpace();
        historial.Estado.Should().Be(EstadoHistorialClinico.Activo);
        historial.Detalles.Should().NotBeNull();
        historial.Detalles.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_HistorialPorPaciente_ConDetalles_DeberiaRetornarHistorialYDetalles()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseHistorial = await TestDataSeeder.CrearPacienteConHistorialYDetalleAsync(
            db,
            dni: "82637415"
        );

        // Act
        var response = await Client.GetAsync($"/api/historiales/paciente/{baseHistorial.Paciente.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var historial = await response.ReadDataAsJsonAsync<HistorialClinicoResponseDto>();

        historial.Should().NotBeNull();
        historial!.Id.Should().Be(baseHistorial.Historial.Id);
        historial.Detalles.Should().NotBeNull();
        historial.Detalles.Should().ContainSingle();

        var detalle = historial.Detalles.Single();

        detalle.Id.Should().Be(baseHistorial.Detalle.Id);
        detalle.HistorialClinicoId.Should().Be(baseHistorial.Historial.Id);
        detalle.TipoMovimiento.Should().Be(TipoMovimientoHistorial.AperturaHistorial);
        detalle.Titulo.Should().Be("Apertura de historial clínico");
        detalle.Descripcion.Should().Be("Se apertura el historial clínico del paciente.");
        detalle.CodigoDetalle.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_HistorialPorPaciente_Inexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var pacienteIdInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/historiales/paciente/{pacienteIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Get_HistorialConDetalles_ExistenteSinDetalles_DeberiaRetornarHistorialConListaVacia()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseHistorial = await TestDataSeeder.CrearPacienteConHistorialAsync(
            db,
            dni: "83741526"
        );

        // Act
        var response = await Client.GetAsync($"/api/historiales/{baseHistorial.Historial.Id}/detalles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var historial = await response.ReadDataAsJsonAsync<HistorialClinicoResponseDto>();

        historial.Should().NotBeNull();
        historial!.Id.Should().Be(baseHistorial.Historial.Id);
        historial.PacienteId.Should().Be(baseHistorial.Paciente.Id);
        historial.Detalles.Should().NotBeNull();
        historial.Detalles.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_HistorialConDetalles_ExistenteConDetalles_DeberiaRetornarDetalles()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseHistorial = await TestDataSeeder.CrearPacienteConHistorialAsync(
            db,
            dni: "84852637"
        );

        var detalle1 = await TestDataSeeder.CrearHistorialDetalleAsync(
            db,
            baseHistorial.Historial.Id,
            tipoMovimiento: TipoMovimientoHistorial.AperturaHistorial,
            titulo: "Apertura de historial",
            descripcion: "Se creó el historial clínico."
        );

        var detalle2 = await TestDataSeeder.CrearHistorialDetalleAsync(
            db,
            baseHistorial.Historial.Id,
            tipoMovimiento: TipoMovimientoHistorial.ObservacionClinica,
            titulo: "Observación clínica",
            descripcion: "Paciente presenta observación registrada."
        );

        // Act
        var response = await Client.GetAsync($"/api/historiales/{baseHistorial.Historial.Id}/detalles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var historial = await response.ReadDataAsJsonAsync<HistorialClinicoResponseDto>();

        historial.Should().NotBeNull();
        historial!.Id.Should().Be(baseHistorial.Historial.Id);
        historial.Detalles.Should().HaveCount(2);

        historial.Detalles.Should().Contain(x =>
            x.Id == detalle1.Id &&
            x.TipoMovimiento == TipoMovimientoHistorial.AperturaHistorial &&
            x.Titulo == "Apertura de historial");

        historial.Detalles.Should().Contain(x =>
            x.Id == detalle2.Id &&
            x.TipoMovimiento == TipoMovimientoHistorial.ObservacionClinica &&
            x.Titulo == "Observación clínica");
    }

    [Fact]
    public async Task Get_HistorialConDetalles_Inexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var historialIdInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/historiales/{historialIdInexistente}/detalles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Get_HistorialConDetalleRelacionadoACita_DeberiaRetornarCitaId()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(
            db,
            baseCita.Paciente.Id
        );

        var cita = await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id
        );

        var detalle = await TestDataSeeder.CrearHistorialDetalleAsync(
            db,
            historial.Id,
            tipoMovimiento: TipoMovimientoHistorial.CitaProgramada,
            titulo: "Cita programada",
            descripcion: "Se programó una cita para el paciente.",
            citaId: cita.Id
        );

        // Act
        var response = await Client.GetAsync($"/api/historiales/{historial.Id}/detalles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var historialResponse = await response.ReadDataAsJsonAsync<HistorialClinicoResponseDto>();

        historialResponse.Should().NotBeNull();
        historialResponse!.Detalles.Should().ContainSingle();

        var detalleResponse = historialResponse.Detalles.Single();

        detalleResponse.Id.Should().Be(detalle.Id);
        detalleResponse.CitaId.Should().Be(cita.Id);
        detalleResponse.TipoMovimiento.Should().Be(TipoMovimientoHistorial.CitaProgramada);
    }
}