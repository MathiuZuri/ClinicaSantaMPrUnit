using System.Net;
using Clinica.API.Controllers;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Atenciones;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.IntegrationTests.PruebasInt.Atenciones;

[Collection("IntegrationTests")]
public class AtencionesEndpointsTests : IntegrationTestBase
{
    public AtencionesEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    // ================================================================
    // AUTENTICACIÓN Y AUTORIZACIÓN
    // ================================================================

    [Fact]
    public async Task Get_Atenciones_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/atenciones");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Atenciones_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var dto = new RegistrarAtencionDto
        {
            PacienteId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            HistorialClinicoId = Guid.NewGuid(),
            CostoFinal = 100
        };
        var response = await Client.PostJsonAsync("/api/atenciones", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_CerrarAtencion_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var dto = new CerrarAtencionDto
        {
            ImpresionDiagnostica = new() { DiagnosticoPrincipal = "DX", IndicacionesReceta = "R" }
        };
        var response = await Client.PutJsonAsync($"/api/atenciones/{Guid.NewGuid()}/cerrar", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_AnularAtencion_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var request = new AnularAtencionRequest { Motivo = "Motivo" };
        var response = await Client.PutJsonAsync($"/api/atenciones/{Guid.NewGuid()}/anular", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================================
    // CONSULTAS
    // ================================================================

    [Fact]
    public async Task Get_Atenciones_ConAdmin_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();

        var response = await Client.GetAsync("/api/atenciones");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_Atenciones_PorPaciente_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "30123456");
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        // Crear una atención para este paciente (sin usar parámetros antiguos)
        await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: await db.HistorialesClinicos.Select(x => x.Id).FirstAsync()
        );

        var response = await Client.GetAsync($"/api/atenciones/paciente/{paciente.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atenciones = await response.ReadDataAsJsonAsync<List<AtencionResponseDto>>();
        atenciones.Should().NotBeNullOrEmpty();
        atenciones!.Should().OnlyContain(x => x.PacienteId == paciente.Id);
    }

    [Fact]
    public async Task Get_Atenciones_PorIdExistente_DeberiaRetornarAtencion()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);
        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id
        );

        var response = await Client.GetAsync($"/api/atenciones/{atencion.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadDataAsJsonAsync<AtencionResponseDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(atencion.Id);
    }

    [Fact]
    public async Task Get_Atenciones_PorIdInexistente_DeberiaRetornarNotFound()
    {
        await LoginAsAdminAsync();

        var response = await Client.GetAsync($"/api/atenciones/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    // ================================================================
    // REGISTRAR
    // ================================================================

    [Fact]
    public async Task Post_Atenciones_Valida_DeberiaCrearAtencion()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);

        var dto = new RegistrarAtencionDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HistorialClinicoId = historial.Id,
            CostoFinal = 150
        };

        var response = await Client.PostJsonAsync("/api/atenciones", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(response);

        var data = await JsonTestHelper.ReadDataAsync(response);
        data.TryGetProperty("id", out var idProp).Should().BeTrue();
        var atencionId = idProp.GetGuid();

        var getResponse = await Client.GetAsync($"/api/atenciones/{atencionId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_AnularAtencion_Abierta_DeberiaAnular()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);
        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id
        );

        var request = new AnularAtencionRequest { Motivo = "Error en el registro" };
        var response = await Client.PutJsonAsync($"/api/atenciones/{atencion.Id}/anular", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var getResponse = await Client.GetAsync($"/api/atenciones/{atencion.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var atencionActualizada = await getResponse.ReadDataAsJsonAsync<AtencionResponseDto>();
        atencionActualizada!.Estado.Should().Be(EstadoAtencion.Anulada);
    }
    
    [Fact]
    public async Task Post_Atenciones_SinHistorialClinico_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new RegistrarAtencionDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HistorialClinicoId = null,          // nulo
            CostoFinal = 100
        };

        var response = await Client.PostJsonAsync("/api/atenciones", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Atenciones_HistorialClinicoVacio_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new RegistrarAtencionDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HistorialClinicoId = Guid.Empty,    // vacío
            CostoFinal = 100
        };

        var response = await Client.PostJsonAsync("/api/atenciones", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}