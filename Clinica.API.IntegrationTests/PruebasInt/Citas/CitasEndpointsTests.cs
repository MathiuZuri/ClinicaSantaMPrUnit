using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Citas;
using Clinica.Domain.Enums;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Citas;

[Collection("IntegrationTests")]
public class CitasEndpointsTests : IntegrationTestBase
{
    public CitasEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Citas_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/citas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Citas_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/citas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Post_Citas_Valida_DeberiaCrearCita()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var dto = new CrearCitaDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HorarioDoctorId = baseCita.Horario.Id,
            Fecha = fecha,
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Control prenatal",
            Observaciones = "Primera cita de control"
        };

        // Act
        var postResponse = await Client.PostJsonAsync("/api/citas", dto);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(postResponse);

        var data = await JsonTestHelper.ReadDataAsync(postResponse);

        data.TryGetProperty("id", out var idProperty)
            .Should()
            .BeTrue("la respuesta de creación debe devolver el id de la cita");

        var citaId = idProperty.GetGuid();

        var getResponse = await Client.GetAsync($"/api/citas/{citaId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cita = await getResponse.ReadDataAsJsonAsync<CitaResponseDto>();

        cita.Should().NotBeNull();
        cita!.Id.Should().Be(citaId);
        cita.PacienteId.Should().Be(dto.PacienteId);
        cita.DoctorId.Should().Be(dto.DoctorId);
        cita.ServicioClinicoId.Should().Be(dto.ServicioClinicoId);
        cita.Fecha.Should().Be(dto.Fecha);
        cita.HoraInicio.Should().Be(dto.HoraInicio);
        cita.HoraFin.Should().Be(dto.HoraFin);
        cita.Motivo.Should().Be(dto.Motivo);
        cita.Observaciones.Should().Be(dto.Observaciones);
        cita.Estado.Should().Be(EstadoCita.Pendiente);
        cita.CodigoCita.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_Citas_PorIdExistente_DeberiaRetornarCita()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var citaCreada = await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id
        );

        // Act
        var response = await Client.GetAsync($"/api/citas/{citaCreada.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var cita = await response.ReadDataAsJsonAsync<CitaResponseDto>();

        cita.Should().NotBeNull();
        cita!.Id.Should().Be(citaCreada.Id);
        cita.PacienteId.Should().Be(baseCita.Paciente.Id);
        cita.DoctorId.Should().Be(baseCita.Doctor.Id);
        cita.ServicioClinicoId.Should().Be(baseCita.Servicio.Id);
    }

    [Fact]
    public async Task Get_Citas_PorIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var idInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/citas/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Get_Citas_PorPaciente_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id
        );

        // Act
        var response = await Client.GetAsync($"/api/citas/paciente/{baseCita.Paciente.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_Citas_PorDoctor_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id
        );

        // Act
        var response = await Client.GetAsync($"/api/citas/doctor/{baseCita.Doctor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Post_Citas_PacienteInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new CrearCitaDto
        {
            PacienteId = Guid.NewGuid(),
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HorarioDoctorId = baseCita.Horario.Id,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Paciente inexistente",
            Observaciones = "Debe fallar"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/citas", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task Post_Citas_DoctorInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new CrearCitaDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = baseCita.Servicio.Id,
            HorarioDoctorId = null,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Doctor inexistente",
            Observaciones = "Debe fallar"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/citas", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task Post_Citas_MotivoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new CrearCitaDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HorarioDoctorId = baseCita.Horario.Id,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "No",
            Observaciones = "Motivo demasiado corto"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/citas", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Citas_ConCruceHorario_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita1 = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var paciente2 = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "71829364",
            nombres: "Paciente",
            apellidos: "Cruce"
        );

        var fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var primeraCita = new CrearCitaDto
        {
            PacienteId = baseCita1.Paciente.Id,
            DoctorId = baseCita1.Doctor.Id,
            ServicioClinicoId = baseCita1.Servicio.Id,
            HorarioDoctorId = baseCita1.Horario.Id,
            Fecha = fecha,
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Primera cita",
            Observaciones = "Cita válida"
        };

        var primeraResponse = await Client.PostJsonAsync("/api/citas", primeraCita);
        primeraResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var segundaCita = new CrearCitaDto
        {
            PacienteId = paciente2.Id,
            DoctorId = baseCita1.Doctor.Id,
            ServicioClinicoId = baseCita1.Servicio.Id,
            HorarioDoctorId = baseCita1.Horario.Id,
            Fecha = fecha,
            HoraInicio = new TimeOnly(9, 15),
            HoraFin = new TimeOnly(9, 45),
            Motivo = "Cita con cruce horario",
            Observaciones = "Debe fallar por cruce"
        };

        // Act
        var segundaResponse = await Client.PostJsonAsync("/api/citas", segundaCita);

        // Assert
        segundaResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(segundaResponse);
    }

    [Fact]
    public async Task Put_Citas_ReprogramarValida_DeberiaActualizarFechaHoraYEstado()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var citaCreada = await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id,
            fecha: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            horaInicio: new TimeOnly(9, 0),
            horaFin: new TimeOnly(9, 30)
        );

        var nuevaFecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));

        var nuevoHorario = await TestDataSeeder.CrearHorarioDoctorAsync(
            db,
            baseCita.Doctor.Id,
            diaSemana: nuevaFecha.DayOfWeek,
            horaInicio: new TimeOnly(8, 0),
            horaFin: new TimeOnly(12, 0)
        );

        var dto = new ReprogramarCitaDto
        {
            DoctorId = baseCita.Doctor.Id,
            HorarioDoctorId = nuevoHorario.Id,
            NuevaFecha = nuevaFecha,
            NuevaHoraInicio = new TimeOnly(10, 0),
            NuevaHoraFin = new TimeOnly(10, 30),
            MotivoReprogramacion = "Cambio solicitado por la paciente"
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/citas/{citaCreada.Id}/reprogramar",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/citas/{citaCreada.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cita = await getResponse.ReadDataAsJsonAsync<CitaResponseDto>();

        cita.Should().NotBeNull();
        cita!.Fecha.Should().Be(dto.NuevaFecha);
        cita.HoraInicio.Should().Be(dto.NuevaHoraInicio);
        cita.HoraFin.Should().Be(dto.NuevaHoraFin);
        cita.Estado.Should().Be(EstadoCita.Reprogramada);
    }

    [Fact]
    public async Task Put_Citas_ReprogramarIdInexistente_DeberiaRetornarNotFoundOBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new ReprogramarCitaDto
        {
            DoctorId = baseCita.Doctor.Id,
            HorarioDoctorId = baseCita.Horario.Id,
            NuevaFecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            NuevaHoraInicio = new TimeOnly(10, 0),
            NuevaHoraFin = new TimeOnly(10, 30),
            MotivoReprogramacion = "Cita inexistente"
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/citas/{Guid.NewGuid()}/reprogramar",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    public async Task Put_Citas_CancelarValida_DeberiaCambiarEstadoACancelada()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var citaCreada = await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id
        );

        var dto = new CancelarCitaDto
        {
            MotivoCancelacion = "La paciente solicitó cancelar la cita"
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/citas/{citaCreada.Id}/cancelar",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/citas/{citaCreada.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cita = await getResponse.ReadDataAsJsonAsync<CitaResponseDto>();

        cita.Should().NotBeNull();
        cita!.Estado.Should().Be(EstadoCita.Cancelada);
    }

    [Fact]
    public async Task Put_Citas_CancelarMotivoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var citaCreada = await TestDataSeeder.CrearCitaAsync(
            db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id
        );

        var dto = new CancelarCitaDto
        {
            MotivoCancelacion = "No"
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/citas/{citaCreada.Id}/cancelar",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Citas_CancelarIdInexistente_DeberiaRetornarNotFoundOBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CancelarCitaDto
        {
            MotivoCancelacion = "Cancelación de cita inexistente"
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/citas/{Guid.NewGuid()}/cancelar",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }
    
    [Fact]
    public async Task Post_Citas_FechaPasada_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new CrearCitaDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HorarioDoctorId = baseCita.Horario.Id,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), // pasada
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Fecha inválida",
            Observaciones = ""
        };

        var response = await Client.PostJsonAsync("/api/citas", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Citas_HoraFinNoMayor_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new CrearCitaDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HorarioDoctorId = baseCita.Horario.Id,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            HoraInicio = new TimeOnly(10, 0),
            HoraFin = new TimeOnly(9, 0), // menor que inicio
            Motivo = "Horario inválido",
            Observaciones = ""
        };

        var response = await Client.PostJsonAsync("/api/citas", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Citas_ReprogramarFechaPasada_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var cita = await TestDataSeeder.CrearCitaAsync(db, 
            pacienteId: baseCita.Paciente.Id, 
            doctorId: baseCita.Doctor.Id, 
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id);

        var dto = new ReprogramarCitaDto
        {
            DoctorId = baseCita.Doctor.Id,
            NuevaFecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), // pasada
            NuevaHoraInicio = new TimeOnly(8, 0),
            NuevaHoraFin = new TimeOnly(8, 30),
            MotivoReprogramacion = "Fecha pasada"
        };

        var response = await Client.PutJsonAsync($"/api/citas/{cita.Id}/reprogramar", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Citas_ReprogramarHoraFinNoMayor_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var cita = await TestDataSeeder.CrearCitaAsync(db, 
            pacienteId: baseCita.Paciente.Id, 
            doctorId: baseCita.Doctor.Id, 
            servicioClinicoId: baseCita.Servicio.Id,
            horarioDoctorId: baseCita.Horario.Id);

        var dto = new ReprogramarCitaDto
        {
            DoctorId = baseCita.Doctor.Id,
            NuevaFecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            NuevaHoraInicio = new TimeOnly(10, 0),
            NuevaHoraFin = new TimeOnly(9, 30), // inválido
            MotivoReprogramacion = "Hora inválida"
        };

        var response = await Client.PutJsonAsync($"/api/citas/{cita.Id}/reprogramar", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_Citas_PorPacienteSinCitas_DeberiaRetornarOkConListaVacia()
    {
        await LoginAsAdminAsync();
        var response = await Client.GetAsync($"/api/citas/paciente/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_Citas_PorDoctorSinCitas_DeberiaRetornarOkConListaVacia()
    {
        await LoginAsAdminAsync();
        var response = await Client.GetAsync($"/api/citas/doctor/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }
}