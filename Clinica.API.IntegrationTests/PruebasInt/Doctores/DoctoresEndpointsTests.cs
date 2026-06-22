using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.DTOs.Usuarios;
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
    
    [Fact]
    public async Task Post_Contratar_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var dto = new ContratarDoctorDto
        {
            CMP = "12345",
            Nombres = "Test",
            Apellidos = "Test",
            Especialidad = "Test",
            UserName = "test",
            CorreoUsuario = "test@test.com",
            Password = "Password123!",
            FechaInicioContrato = DateTime.UtcNow
        };
        var response = await Client.PostJsonAsync("/api/doctores/contratar", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_BuscarDoctores_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/doctores/buscar?pagina=1&cantidadPorPagina=5");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Post_Contratar_Valido_DeberiaCrearDoctorYUsuario()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new ContratarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "Carlos",
            Apellidos = "Mamani Flores",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = $"carlos.mamani.{Guid.NewGuid():N}@test.com",
            FechaInicioContrato = DateTime.UtcNow,
            UserName = $"carlos_{Guid.NewGuid():N}"[..20],
            CorreoUsuario = $"carlos.usuario.{Guid.NewGuid():N}@test.com",
            Password = "Password123!"
        };

        // Act
        var postResponse = await Client.PostJsonAsync("/api/doctores/contratar", dto);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(postResponse);

        var data = await JsonTestHelper.ReadDataAsync(postResponse);
        data.TryGetProperty("id", out var idProperty)
            .Should().BeTrue("la respuesta debe devolver el id del doctor");

        var doctorId = idProperty.GetGuid();

        // Verificar que el doctor existe
        var getResponse = await Client.GetAsync($"/api/doctores/{doctorId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var doctor = await getResponse.ReadDataAsJsonAsync<DoctorResponseDto>();
        doctor.Should().NotBeNull();
        doctor!.CMP.Should().Be(dto.CMP);
        doctor.Nombres.Should().Be(dto.Nombres);
        doctor.Apellidos.Should().Be(dto.Apellidos);
        doctor.Especialidad.Should().Be(dto.Especialidad);

        // Verificar que el usuario fue creado (podemos consultar la lista de usuarios)
        var usuariosResponse = await Client.GetAsync("/api/usuarios");
        var usuarios = await usuariosResponse.ReadDataAsJsonAsync<List<UsuarioResponseDto>>();
        usuarios.Should().Contain(u => u.UserName == dto.UserName && u.Correo == dto.CorreoUsuario);
    }
    
    [Fact]
    public async Task Post_Contratar_CmpDuplicado_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        var cmp = $"CMP{Random.Shared.Next(100000, 999999)}";
        await using (var db = CreateDbContext())
        {
            await TestDataSeeder.CrearDoctorAsync(db, cmp: cmp);
        }

        var dto = new ContratarDoctorDto
        {
            CMP = cmp,
            Nombres = "Otro",
            Apellidos = "Doctor",
            Especialidad = "Pediatría",
            UserName = $"otro_{Guid.NewGuid():N}"[..20],
            CorreoUsuario = $"otro.{Guid.NewGuid():N}@test.com",
            Password = "Password123!",
            FechaInicioContrato = DateTime.UtcNow
        };

        var response = await Client.PostJsonAsync("/api/doctores/contratar", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Contratar_UserNameExistente_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        // Usar el nombre de usuario de admin que ya existe
        var dto = new ContratarDoctorDto
        {
            CMP = $"CMP{Random.Shared.Next(100000, 999999)}",
            Nombres = "Test",
            Apellidos = "User",
            Especialidad = "Test",
            UserName = "admin", // ya existe
            CorreoUsuario = $"nuevo.{Guid.NewGuid():N}@test.com",
            Password = "Password123!",
            FechaInicioContrato = DateTime.UtcNow
        };

        var response = await Client.PostJsonAsync("/api/doctores/contratar", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Get_BuscarDoctores_SinFiltros_DeberiaRetornarPaginado()
    {
        await LoginAsAdminAsync();

        var response = await Client.GetAsync("/api/doctores/buscar?pagina=1&cantidadPorPagina=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<DoctorResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Pagina.Should().Be(1);
        paginado.CantidadPorPagina.Should().Be(5);
        paginado.Datos.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_BuscarDoctores_PorNombre_DeberiaFiltrar()
    {
        await LoginAsAdminAsync();

        // Crear un doctor con nombre específico
        await using var db = CreateDbContext();
        var doctor = await TestDataSeeder.CrearDoctorAsync(db,
            cmp: $"CMP{Random.Shared.Next(100000, 999999)}",
            nombres: "Zulema",
            apellidos: "Unica",
            especialidad: "Obstetricia");

        var response = await Client.GetAsync($"/api/doctores/buscar?nombre=Zulema&pagina=1&cantidadPorPagina=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<DoctorResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Datos.Should().Contain(d => d.Id == doctor.Id);
    }

    [Fact]
    public async Task Get_BuscarDoctores_PorEstado_DeberiaFiltrarSoloActivos()
    {
        await LoginAsAdminAsync();

        var response = await Client.GetAsync($"/api/doctores/buscar?estado={(int)EstadoDoctor.Activo}&pagina=1&cantidadPorPagina=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<DoctorResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Datos.Should().OnlyContain(d => d.Estado == EstadoDoctor.Activo);
    }
    
    
}