using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Pacientes;
using Clinica.Domain.Enums;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Pacientes;

[Collection("IntegrationTests")]
public class PacientesEndpointsTests : IntegrationTestBase
{
    public PacientesEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Pacientes_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/pacientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Pacientes_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/pacientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Post_Pacientes_Valido_DeberiaCrearPaciente()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearPacienteDto
        {
            DNI = "76543210",
            Nombres = "Rosa",
            Apellidos = "Mamani Quispe",
            FechaNacimiento = new DateTime(1999, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Sexo = "F",
            Celular = "987654321",
            Correo = "rosa.mamani@test.com",
            Direccion = "Jr. Lima 123"
        };

        // Act
        var postResponse = await Client.PostJsonAsync("/api/pacientes", dto);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(postResponse);

        var data = await JsonTestHelper.ReadDataAsync(postResponse);

        data.TryGetProperty("id", out var idProperty)
            .Should()
            .BeTrue("la respuesta de creación debe devolver el id del paciente");

        var pacienteId = idProperty.GetGuid();

        var getResponse = await Client.GetAsync($"/api/pacientes/{pacienteId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var paciente = await getResponse.ReadDataAsJsonAsync<PacienteResponseDto>();

        paciente.Should().NotBeNull();
        paciente!.Id.Should().Be(pacienteId);
        paciente.DNI.Should().Be(dto.DNI);
        paciente.Nombres.Should().Be(dto.Nombres);
        paciente.Apellidos.Should().Be(dto.Apellidos);
        paciente.Sexo.Should().Be(dto.Sexo);
        paciente.Celular.Should().Be(dto.Celular);
        paciente.Correo.Should().Be(dto.Correo);
        paciente.Direccion.Should().Be(dto.Direccion);
        paciente.Estado.Should().Be(EstadoPaciente.Activo);
        paciente.CodigoPaciente.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_Pacientes_PorIdExistente_DeberiaRetornarPaciente()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var pacienteCreado = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "12345670",
            nombres: "Lucia",
            apellidos: "Condori",
            sexo: "F"
        );

        // Act
        var response = await Client.GetAsync($"/api/pacientes/{pacienteCreado.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var paciente = await response.ReadDataAsJsonAsync<PacienteResponseDto>();

        paciente.Should().NotBeNull();
        paciente!.Id.Should().Be(pacienteCreado.Id);
        paciente.DNI.Should().Be("12345670");
        paciente.Nombres.Should().Be("Lucia");
        paciente.Apellidos.Should().Be("Condori");
    }

    [Fact]
    public async Task Get_Pacientes_PorIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var idInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/pacientes/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Get_Pacientes_PorDniExistente_DeberiaRetornarPaciente()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var pacienteCreado = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "45678912",
            nombres: "Carmen",
            apellidos: "Flores",
            sexo: "F"
        );

        // Act
        var response = await Client.GetAsync("/api/pacientes/dni/45678912");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var paciente = await response.ReadDataAsJsonAsync<PacienteResponseDto>();

        paciente.Should().NotBeNull();
        paciente!.Id.Should().Be(pacienteCreado.Id);
        paciente.DNI.Should().Be("45678912");
    }

    [Fact]
    public async Task Get_Pacientes_PorDniInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/pacientes/dni/99999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Pacientes_DniInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearPacienteDto
        {
            DNI = "123",
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(1998, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            Sexo = "F",
            Celular = "987654321",
            Correo = "ana@test.com",
            Direccion = "Jr. Lima 123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pacientes", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pacientes_SexoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearPacienteDto
        {
            DNI = "22223333",
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(1998, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            Sexo = "X",
            Celular = "987654321",
            Correo = "ana@test.com",
            Direccion = "Jr. Lima 123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pacientes", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pacientes_CelularInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearPacienteDto
        {
            DNI = "33334444",
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(1998, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            Sexo = "F",
            Celular = "123",
            Correo = "ana@test.com",
            Direccion = "Jr. Lima 123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pacientes", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Pacientes_CorreoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearPacienteDto
        {
            DNI = "44445555",
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(1998, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            Sexo = "F",
            Celular = "987654321",
            Correo = "correo_invalido",
            Direccion = "Jr. Lima 123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/pacientes", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_PacienteContacto_Valido_DeberiaActualizarContacto()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var pacienteCreado = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "56789123",
            nombres: "Mariela",
            apellidos: "Paredes",
            sexo: "F"
        );

        var dto = new ActualizarContactoPacienteDto
        {
            Celular = "912345678",
            Correo = "mariela.actualizada@test.com",
            Direccion = "Av. Circunvalación 456"
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/pacientes/{pacienteCreado.Id}/contacto",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/pacientes/{pacienteCreado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var paciente = await getResponse.ReadDataAsJsonAsync<PacienteResponseDto>();

        paciente.Should().NotBeNull();
        paciente!.Celular.Should().Be(dto.Celular);
        paciente.Correo.Should().Be(dto.Correo);
        paciente.Direccion.Should().Be(dto.Direccion);
    }

    [Fact]
    public async Task Put_PacienteContacto_IdInexistente_DeberiaRetornarNotFoundOBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var idInexistente = Guid.NewGuid();

        var dto = new ActualizarContactoPacienteDto
        {
            Celular = "912345678",
            Correo = "noexiste@test.com",
            Direccion = "Dirección de prueba"
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/pacientes/{idInexistente}/contacto",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    public async Task Put_PacienteContacto_CelularInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var pacienteCreado = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "67891234"
        );

        var dto = new ActualizarContactoPacienteDto
        {
            Celular = "123",
            Correo = "valido@test.com",
            Direccion = "Dirección válida"
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/pacientes/{pacienteCreado.Id}/contacto",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Put_CambiarEstado_Valido_DeberiaActualizarEstado()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "78901234",
            nombres: "Estado",
            apellidos: "Test",
            sexo: "F"
        );

        var dto = new CambiarEstadoPacienteDto { Estado = EstadoPaciente.Inactivo };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/pacientes/{paciente.Id}/estado",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/pacientes/{paciente.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pacienteActualizado = await getResponse.ReadDataAsJsonAsync<PacienteResponseDto>();
        pacienteActualizado!.Estado.Should().Be(EstadoPaciente.Inactivo);
    }
    [Fact]
    public async Task Put_CambiarEstado_IdInexistente_DeberiaRetornarNotFoundOBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CambiarEstadoPacienteDto { Estado = EstadoPaciente.Inactivo };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/pacientes/{Guid.NewGuid()}/estado",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }
    [Fact]
    public async Task Put_CambiarEstado_PacienteEliminado_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        // Crear paciente y marcarlo como eliminado directamente en BD
        var paciente = await TestDataSeeder.CrearPacienteAsync(
            db,
            dni: "89012345",
            nombres: "Eliminado",
            apellidos: "Test",
            sexo: "M"
        );
        paciente.Estado = EstadoPaciente.Eliminado;
        await db.SaveChangesAsync();

        var dto = new CambiarEstadoPacienteDto { Estado = EstadoPaciente.Activo };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/pacientes/{paciente.Id}/estado",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}