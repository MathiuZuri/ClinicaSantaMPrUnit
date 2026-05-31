using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Horarios;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Horarios;

[Collection("IntegrationTests")]
public class HorariosEndpointsTests : IntegrationTestBase
{
    public HorariosEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Horarios_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/horarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Horarios_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/horarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Post_Horarios_Valido_DeberiaCrearHorario()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctor = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var fechaInicio = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var dto = new CrearHorarioDoctorDto
        {
            DoctorId = doctor.Id,
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(12, 0),
            FechaInicioVigencia = fechaInicio,
            FechaFinVigencia = null
        };

        // Act
        var postResponse = await Client.PostJsonAsync("/api/horarios", dto);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(postResponse);

        var data = await JsonTestHelper.ReadDataAsync(postResponse);

        data.TryGetProperty("id", out var idProperty)
            .Should()
            .BeTrue("la respuesta de creación debe devolver el id del horario");

        var horarioId = idProperty.GetGuid();

        var getResponse = await Client.GetAsync($"/api/horarios/doctor/{doctor.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var horarios = await getResponse.ReadDataAsJsonAsync<List<HorarioDoctorResponseDto>>();

        horarios.Should().NotBeNull();
        horarios!.Should().Contain(x =>
            x.Id == horarioId &&
            x.DoctorId == doctor.Id &&
            x.DiaSemana == dto.DiaSemana &&
            x.HoraInicio == dto.HoraInicio &&
            x.HoraFin == dto.HoraFin &&
            x.Activo
        );
    }

    [Fact]
    public async Task Get_Horarios_PorDoctor_DeberiaRetornarHorariosDelDoctor()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctor = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var horarioCreado = await TestDataSeeder.CrearHorarioDoctorAsync(
            db,
            doctor.Id,
            diaSemana: DayOfWeek.Tuesday,
            horaInicio: new TimeOnly(9, 0),
            horaFin: new TimeOnly(13, 0)
        );

        // Act
        var response = await Client.GetAsync($"/api/horarios/doctor/{doctor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var horarios = await response.ReadDataAsJsonAsync<List<HorarioDoctorResponseDto>>();

        horarios.Should().NotBeNull();
        horarios!.Should().Contain(x =>
            x.Id == horarioCreado.Id &&
            x.DoctorId == doctor.Id &&
            x.DiaSemana == DayOfWeek.Tuesday
        );
    }

    [Fact]
    public async Task Get_Horarios_PorDoctorSinHorarios_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctor = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        // Act
        var response = await Client.GetAsync($"/api/horarios/doctor/{doctor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var horarios = await response.ReadDataAsJsonAsync<List<HorarioDoctorResponseDto>>();

        horarios.Should().NotBeNull();
        horarios.Should().BeEmpty();
    }

    [Fact]
    public async Task Post_Horarios_DoctorInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearHorarioDoctorDto
        {
            DoctorId = Guid.NewGuid(),
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(12, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            FechaFinVigencia = null
        };

        // Act
        var response = await Client.PostJsonAsync("/api/horarios", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task Post_Horarios_DoctorVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearHorarioDoctorDto
        {
            DoctorId = Guid.Empty,
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(12, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            FechaFinVigencia = null
        };

        // Act
        var response = await Client.PostJsonAsync("/api/horarios", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Horarios_HoraFinMenorQueInicio_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctor = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var dto = new CrearHorarioDoctorDto
        {
            DoctorId = doctor.Id,
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(12, 0),
            HoraFin = new TimeOnly(8, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            FechaFinVigencia = null
        };

        // Act
        var response = await Client.PostJsonAsync("/api/horarios", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Horarios_Valido_DeberiaActualizarHorario()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctor = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var horarioCreado = await TestDataSeeder.CrearHorarioDoctorAsync(
            db,
            doctor.Id,
            diaSemana: DayOfWeek.Monday,
            horaInicio: new TimeOnly(8, 0),
            horaFin: new TimeOnly(12, 0)
        );

        var dto = new EditarHorarioDoctorDto
        {
            DiaSemana = DayOfWeek.Wednesday,
            HoraInicio = new TimeOnly(14, 0),
            HoraFin = new TimeOnly(18, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            FechaFinVigencia = null,
            Activo = true
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/horarios/{horarioCreado.Id}",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/horarios/doctor/{doctor.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var horarios = await getResponse.ReadDataAsJsonAsync<List<HorarioDoctorResponseDto>>();

        horarios.Should().NotBeNull();

        var horarioActualizado = horarios!.FirstOrDefault(x => x.Id == horarioCreado.Id);

        horarioActualizado.Should().NotBeNull();
        horarioActualizado!.DiaSemana.Should().Be(dto.DiaSemana);
        horarioActualizado.HoraInicio.Should().Be(dto.HoraInicio);
        horarioActualizado.HoraFin.Should().Be(dto.HoraFin);
        horarioActualizado.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Put_Horarios_Inactivar_DeberiaActualizarActivoFalse()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctor = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var horarioCreado = await TestDataSeeder.CrearHorarioDoctorAsync(
            db,
            doctor.Id
        );

        var dto = new EditarHorarioDoctorDto
        {
            DiaSemana = horarioCreado.DiaSemana,
            HoraInicio = horarioCreado.HoraInicio,
            HoraFin = horarioCreado.HoraFin,
            FechaInicioVigencia = horarioCreado.FechaInicioVigencia,
            FechaFinVigencia = horarioCreado.FechaFinVigencia,
            Activo = false
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/horarios/{horarioCreado.Id}",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/horarios/doctor/{doctor.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var horarios = await getResponse.ReadDataAsJsonAsync<List<HorarioDoctorResponseDto>>();

        horarios.Should().NotBeNull();

        var horarioActualizado = horarios!.FirstOrDefault(x => x.Id == horarioCreado.Id);

        horarioActualizado.Should().NotBeNull();
        horarioActualizado.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task Put_Horarios_IdInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new EditarHorarioDoctorDto
        {
            DiaSemana = DayOfWeek.Friday,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(12, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            FechaFinVigencia = null,
            Activo = true
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/horarios/{Guid.NewGuid()}",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task Put_Horarios_HoraFinMenorQueInicio_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctor = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var horarioCreado = await TestDataSeeder.CrearHorarioDoctorAsync(
            db,
            doctor.Id
        );

        var dto = new EditarHorarioDoctorDto
        {
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(12, 0),
            HoraFin = new TimeOnly(8, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            FechaFinVigencia = null,
            Activo = true
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/horarios/{horarioCreado.Id}",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Get_MatrizSemanal_SinFecha_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db, cmp: $"CMP{Random.Shared.Next(100000, 999999)}");

        var response = await Client.GetAsync($"/api/horarios/doctor/{doctor.Id}/matriz");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }
    [Fact]
    public async Task Get_MatrizSemanal_ConFecha_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db, cmp: $"CMP{Random.Shared.Next(100000, 999999)}");

        var fecha = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");
        var response = await Client.GetAsync($"/api/horarios/doctor/{doctor.Id}/matriz?fecha={fecha}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }
}