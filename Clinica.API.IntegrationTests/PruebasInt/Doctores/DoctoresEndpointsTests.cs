using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.Enums;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Doctores;

[Collection("IntegrationTests")]
public class DoctoresEndpointsTests : IntegrationTestBase
{
    public DoctoresEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Doctores_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/doctores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Doctores_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/doctores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_DoctoresActivos_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/doctores/activos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Post_Doctores_Valido_DeberiaCrearDoctor()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "López Quispe",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = $"maria.lopez.{Guid.NewGuid():N}@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1),
            FechaFinContrato = null
        };

        // Act
        var postResponse = await Client.PostJsonAsync("/api/doctores", dto);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(postResponse);

        var data = await JsonTestHelper.ReadDataAsync(postResponse);

        data.TryGetProperty("id", out var idProperty)
            .Should()
            .BeTrue("la respuesta de creación debe devolver el id del doctor");

        var doctorId = idProperty.GetGuid();

        var getResponse = await Client.GetAsync($"/api/doctores/{doctorId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var doctor = await getResponse.ReadDataAsJsonAsync<DoctorResponseDto>();

        doctor.Should().NotBeNull();
        doctor!.Id.Should().Be(doctorId);
        doctor.CMP.Should().Be(dto.CMP);
        doctor.Nombres.Should().Be(dto.Nombres);
        doctor.Apellidos.Should().Be(dto.Apellidos);
        doctor.Especialidad.Should().Be(dto.Especialidad);
        doctor.Celular.Should().Be(dto.Celular);
        doctor.Correo.Should().Be(dto.Correo);
        doctor.Estado.Should().Be(EstadoDoctor.Activo);
        doctor.CodigoDoctor.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_Doctores_PorIdExistente_DeberiaRetornarDoctor()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctorCreado = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}",
            nombres: "Roxana",
            apellidos: "Condori",
            especialidad: "Ginecología"
        );

        // Act
        var response = await Client.GetAsync($"/api/doctores/{doctorCreado.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var doctor = await response.ReadDataAsJsonAsync<DoctorResponseDto>();

        doctor.Should().NotBeNull();
        doctor!.Id.Should().Be(doctorCreado.Id);
        doctor.CMP.Should().Be(doctorCreado.CMP);
        doctor.Nombres.Should().Be("Roxana");
        doctor.Apellidos.Should().Be("Condori");
        doctor.Especialidad.Should().Be("Ginecología");
    }

    [Fact]
    public async Task Get_Doctores_PorIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var idInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/doctores/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Doctores_CmpInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = "AB",
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "doctor@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1)
        };

        // Act
        var response = await Client.PostJsonAsync("/api/doctores", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Doctores_NombresInvalidos_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "A",
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "doctor@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1)
        };

        // Act
        var response = await Client.PostJsonAsync("/api/doctores", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Doctores_EspecialidadInvalida_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "OB",
            Celular = "987654321",
            Correo = "doctor@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1)
        };

        // Act
        var response = await Client.PostJsonAsync("/api/doctores", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Doctores_CelularInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "123",
            Correo = "doctor@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1)
        };

        // Act
        var response = await Client.PostJsonAsync("/api/doctores", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Doctores_CorreoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "correo_invalido",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1)
        };

        // Act
        var response = await Client.PostJsonAsync("/api/doctores", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Doctores_Valido_DeberiaActualizarDoctor()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctorCreado = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}",
            nombres: "Laura",
            apellidos: "Mamani",
            especialidad: "Obstetricia"
        );

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "Laura Actualizada",
            Apellidos = "Mamani Flores",
            Especialidad = "Ginecología",
            Celular = "912345678",
            Correo = $"laura.actualizada.{Guid.NewGuid():N}@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-2),
            FechaFinContrato = null,
            Estado = EstadoDoctor.Activo
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/doctores/{doctorCreado.Id}",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/doctores/{doctorCreado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var doctor = await getResponse.ReadDataAsJsonAsync<DoctorResponseDto>();

        doctor.Should().NotBeNull();
        doctor!.Id.Should().Be(doctorCreado.Id);
        doctor.CMP.Should().Be(dto.CMP);
        doctor.Nombres.Should().Be(dto.Nombres);
        doctor.Apellidos.Should().Be(dto.Apellidos);
        doctor.Especialidad.Should().Be(dto.Especialidad);
        doctor.Celular.Should().Be(dto.Celular);
        doctor.Correo.Should().Be(dto.Correo);
        doctor.Estado.Should().Be(dto.Estado);
    }

    [Fact]
    public async Task Put_Doctores_IdInexistente_DeberiaRetornarNotFoundOBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "Doctor",
            Apellidos = "Inexistente",
            Especialidad = "Obstetricia",
            Celular = "912345678",
            Correo = "doctor.inexistente@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1),
            FechaFinContrato = null,
            Estado = EstadoDoctor.Activo
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/doctores/{Guid.NewGuid()}",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    public async Task Put_Doctores_CelularInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctorCreado = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "Doctor",
            Apellidos = "Prueba",
            Especialidad = "Obstetricia",
            Celular = "123",
            Correo = "doctor@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1),
            FechaFinContrato = null,
            Estado = EstadoDoctor.Activo
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/doctores/{doctorCreado.Id}",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Doctores_CorreoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var doctorCreado = await TestDataSeeder.CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}"
        );

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "Doctor",
            Apellidos = "Prueba",
            Especialidad = "Obstetricia",
            Celular = "912345678",
            Correo = "correo_invalido",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1),
            FechaFinContrato = null,
            Estado = EstadoDoctor.Activo
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/doctores/{doctorCreado.Id}",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Post_Doctores_CmpDuplicado_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();
        var cmp = $"CMP{Random.Shared.Next(100000, 999999)}";

        // Crear primer doctor con ese CMP
        await using (var db = CreateDbContext())
        {
            await TestDataSeeder.CrearDoctorAsync(db, cmp: cmp);
        }

        var dto = new CrearDoctorDto
        {
            CMP = cmp,
            Nombres = "Otro",
            Apellidos = "Doctor",
            Especialidad = "Pediatría",
            Celular = "987654321",
            Correo = "otro@test.com",
            FechaInicioContrato = DateTime.UtcNow
        };

        // Act
        var response = await Client.PostJsonAsync("/api/doctores", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Post_Doctores_FechaFinMenorFechaInicio_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "maria@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(1),
            FechaFinContrato = DateTime.UtcNow   // antes que inicio
        };

        var response = await Client.PostJsonAsync("/api/doctores", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Post_Doctores_ApellidosInvalidos_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        var dto = new CrearDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "A",   // demasiado corto
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "maria@test.com",
            FechaInicioContrato = DateTime.UtcNow
        };

        var response = await Client.PostJsonAsync("/api/doctores", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Put_Doctores_CmpInvalido_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db, cmp: $"CMP{Random.Shared.Next(100000, 999999)}");

        var dto = new EditarDoctorDto
        {
            CMP = "AB",   // muy corto
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "maria@test.com",
            FechaInicioContrato = DateTime.UtcNow,
            Estado = EstadoDoctor.Activo
        };

        var response = await Client.PutJsonAsync($"/api/doctores/{doctor.Id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Put_Doctores_NombresInvalidos_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db, cmp: $"CMP{Random.Shared.Next(100000, 999999)}");

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "A",   // muy corto
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "maria@test.com",
            FechaInicioContrato = DateTime.UtcNow,
            Estado = EstadoDoctor.Activo
        };

        var response = await Client.PutJsonAsync($"/api/doctores/{doctor.Id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Put_Doctores_ApellidosInvalidos_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db, cmp: $"CMP{Random.Shared.Next(100000, 999999)}");

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "A",   // muy corto
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "maria@test.com",
            FechaInicioContrato = DateTime.UtcNow,
            Estado = EstadoDoctor.Activo
        };

        var response = await Client.PutJsonAsync($"/api/doctores/{doctor.Id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Put_Doctores_EspecialidadInvalida_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db, cmp: $"CMP{Random.Shared.Next(100000, 999999)}");

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "OB",   // muy corta
            Celular = "987654321",
            Correo = "maria@test.com",
            FechaInicioContrato = DateTime.UtcNow,
            Estado = EstadoDoctor.Activo
        };

        var response = await Client.PutJsonAsync($"/api/doctores/{doctor.Id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Put_Doctores_FechaFinMenorFechaInicio_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db, cmp: $"CMP{Random.Shared.Next(100000, 999999)}");

        var dto = new EditarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "María",
            Apellidos = "López",
            Especialidad = "Obstetricia",
            Celular = "987654321",
            Correo = "maria@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(2),
            FechaFinContrato = DateTime.UtcNow,   // antes que inicio
            Estado = EstadoDoctor.Activo
        };

        var response = await Client.PutJsonAsync($"/api/doctores/{doctor.Id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
}