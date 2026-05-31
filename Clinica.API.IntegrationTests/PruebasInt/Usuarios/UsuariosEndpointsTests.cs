using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Usuarios;
using Clinica.Domain.Enums;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Usuarios;

[Collection("IntegrationTests")]
public class UsuariosEndpointsTests : IntegrationTestBase
{
    public UsuariosEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Usuarios_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Usuarios_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_Usuarios_DeberiaIncluirAdminDelSeeder()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var usuarios = await response.ReadDataAsJsonAsync<List<UsuarioResponseDto>>();

        usuarios.Should().NotBeNull();
        usuarios.Should().NotBeEmpty();

        usuarios!.Should().Contain(x =>
            x.UserName == "admin" &&
            x.Correo == "admin@clinica.com" &&
            x.Estado == EstadoUsuario.Activo
        );
    }

    [Fact]
    public async Task Post_Usuarios_Valido_DeberiaCrearUsuario()
    {
        // Arrange
        await LoginAsAdminAsync();

        var userName = $"user_{Guid.NewGuid():N}"[..18];
        var correo = $"{userName}@test.com";

        var dto = new CrearUsuarioDto
        {
            Nombres = "Carlos",
            Apellidos = "Mamani Flores",
            UserName = userName,
            Correo = correo,
            Password = "Password123"
        };

        // Act
        var postResponse = await Client.PostJsonAsync("/api/usuarios", dto);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(postResponse);

        var data = await JsonTestHelper.ReadDataAsync(postResponse);

        data.TryGetProperty("id", out var idProperty)
            .Should()
            .BeTrue("la respuesta de creación debe devolver el id del usuario");

        var usuarioId = idProperty.GetGuid();

        var getResponse = await Client.GetAsync($"/api/usuarios/{usuarioId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var usuario = await getResponse.ReadDataAsJsonAsync<UsuarioResponseDto>();

        usuario.Should().NotBeNull();
        usuario!.Id.Should().Be(usuarioId);
        usuario.Nombres.Should().Be(dto.Nombres);
        usuario.Apellidos.Should().Be(dto.Apellidos);
        usuario.UserName.Should().Be(dto.UserName);
        usuario.Correo.Should().Be(dto.Correo);
        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        usuario.CodigoUsuario.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_Usuarios_PorIdExistente_DeberiaRetornarUsuario()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var usuarioCreado = await TestDataSeeder.CrearUsuarioAsync(
            db,
            userName: $"user_{Guid.NewGuid():N}"[..18],
            correo: $"usuario_{Guid.NewGuid():N}@test.com",
            password: "Password123",
            nombres: "Lucia",
            apellidos: "Quispe"
        );

        // Act
        var response = await Client.GetAsync($"/api/usuarios/{usuarioCreado.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var usuario = await response.ReadDataAsJsonAsync<UsuarioResponseDto>();

        usuario.Should().NotBeNull();
        usuario!.Id.Should().Be(usuarioCreado.Id);
        usuario.Nombres.Should().Be("Lucia");
        usuario.Apellidos.Should().Be("Quispe");
        usuario.UserName.Should().Be(usuarioCreado.UserName);
        usuario.Correo.Should().Be(usuarioCreado.Correo);
    }

    [Fact]
    public async Task Get_Usuarios_PorIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var idInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/usuarios/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Usuarios_UserNameInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearUsuarioDto
        {
            Nombres = "Carlos",
            Apellidos = "Mamani",
            UserName = "us",
            Correo = "carlos@test.com",
            Password = "Password123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Usuarios_UserNameConCaracteresInvalidos_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearUsuarioDto
        {
            Nombres = "Carlos",
            Apellidos = "Mamani",
            UserName = "usuario inválido",
            Correo = "carlos2@test.com",
            Password = "Password123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Usuarios_CorreoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearUsuarioDto
        {
            Nombres = "Carlos",
            Apellidos = "Mamani",
            UserName = $"user_{Guid.NewGuid():N}"[..18],
            Correo = "correo_invalido",
            Password = "Password123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Usuarios_PasswordCorto_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearUsuarioDto
        {
            Nombres = "Carlos",
            Apellidos = "Mamani",
            UserName = $"user_{Guid.NewGuid():N}"[..18],
            Correo = $"usuario_{Guid.NewGuid():N}@test.com",
            Password = "123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Usuarios_Valido_DeberiaActualizarUsuario()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var usuarioCreado = await TestDataSeeder.CrearUsuarioAsync(
            db,
            userName: $"user_{Guid.NewGuid():N}"[..18],
            correo: $"usuario_{Guid.NewGuid():N}@test.com",
            password: "Password123",
            nombres: "Luis",
            apellidos: "Condori"
        );

        var nuevoUserName = $"edit_{Guid.NewGuid():N}"[..18];
        var nuevoCorreo = $"{nuevoUserName}@test.com";

        var dto = new EditarUsuarioDto
        {
            Nombres = "Luis Actualizado",
            Apellidos = "Condori Mamani",
            UserName = nuevoUserName,
            Correo = nuevoCorreo
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/usuarios/{usuarioCreado.Id}",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/usuarios/{usuarioCreado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var usuario = await getResponse.ReadDataAsJsonAsync<UsuarioResponseDto>();

        usuario.Should().NotBeNull();
        usuario!.Id.Should().Be(usuarioCreado.Id);
        usuario.Nombres.Should().Be(dto.Nombres);
        usuario.Apellidos.Should().Be(dto.Apellidos);
        usuario.UserName.Should().Be(dto.UserName);
        usuario.Correo.Should().Be(dto.Correo);
    }

    [Fact]
    public async Task Put_Usuarios_IdInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new EditarUsuarioDto
        {
            Nombres = "Usuario",
            Apellidos = "Inexistente",
            UserName = $"user_{Guid.NewGuid():N}"[..18],
            Correo = $"inexistente_{Guid.NewGuid():N}@test.com"
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/usuarios/{Guid.NewGuid()}",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    public async Task Put_Usuarios_CorreoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var usuarioCreado = await TestDataSeeder.CrearUsuarioAsync(
            db,
            userName: $"user_{Guid.NewGuid():N}"[..18],
            correo: $"usuario_{Guid.NewGuid():N}@test.com",
            password: "Password123"
        );

        var dto = new EditarUsuarioDto
        {
            Nombres = "Usuario",
            Apellidos = "Prueba",
            UserName = $"edit_{Guid.NewGuid():N}"[..18],
            Correo = "correo_invalido"
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/usuarios/{usuarioCreado.Id}",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Usuarios_AsignarRolValido_DeberiaAsignarRol()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var usuario = await TestDataSeeder.CrearUsuarioAsync(
            db,
            userName: $"roluser_{Guid.NewGuid():N}"[..18],
            correo: $"roluser_{Guid.NewGuid():N}@test.com",
            password: "Password123"
        );

        var rol = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Asignacion Test",
            permisos: null
        );

        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = usuario.Id,
            RolId = rol.Id
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios/asignar-rol", dto);

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"el backend respondió: {body}"
        );
        await JsonTestHelper.AssertSuccessAsync(response);

        var existeRelacion = db.UsuarioRoles.Any(x =>
            x.UsuarioId == usuario.Id &&
            x.RolId == rol.Id &&
            x.Activo);

        existeRelacion.Should().BeTrue();
    }

    [Fact]
    public async Task Post_Usuarios_AsignarRolUsuarioVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rol = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Usuario Vacio Test",
            permisos: null
        );

        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = Guid.Empty,
            RolId = rol.Id
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios/asignar-rol", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Usuarios_AsignarRolRolVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var usuario = await TestDataSeeder.CrearUsuarioAsync(
            db,
            userName: $"roluser_{Guid.NewGuid():N}"[..18],
            correo: $"roluser_{Guid.NewGuid():N}@test.com",
            password: "Password123"
        );

        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = usuario.Id,
            RolId = Guid.Empty
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios/asignar-rol", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Usuarios_AsignarRolUsuarioInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rol = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Usuario Inexistente Test",
            permisos: null
        );

        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = Guid.NewGuid(),
            RolId = rol.Id
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios/asignar-rol", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task Post_Usuarios_AsignarRolRolInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var usuario = await TestDataSeeder.CrearUsuarioAsync(
            db,
            userName: $"roluser_{Guid.NewGuid():N}"[..18],
            correo: $"roluser_{Guid.NewGuid():N}@test.com",
            password: "Password123"
        );

        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = usuario.Id,
            RolId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostJsonAsync("/api/usuarios/asignar-rol", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        );
    }
    [Fact]
    public async Task Put_CambiarEstado_Valido_DeberiaActualizarEstado()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var usuario = await TestDataSeeder.CrearUsuarioAsync(
            db,
            userName: $"estado_{Guid.NewGuid():N}"[..18],
            correo: $"estado_{Guid.NewGuid():N}@test.com",
            password: "Password123"
        );

        var dto = new CambiarEstadoUsuarioDto { Estado = EstadoUsuario.Inactivo };

        var putResponse = await Client.PutJsonAsync($"/api/usuarios/{usuario.Id}/estado", dto);

        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/usuarios/{usuario.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var usuarioActualizado = await getResponse.ReadDataAsJsonAsync<UsuarioResponseDto>();
        usuarioActualizado!.Estado.Should().Be(EstadoUsuario.Inactivo);
    }

    [Fact]
    public async Task Put_CambiarEstado_IdInexistente_DeberiaRetornarNotFoundOBadRequest()
    {
        await LoginAsAdminAsync();

        var dto = new CambiarEstadoUsuarioDto { Estado = EstadoUsuario.Inactivo };

        var response = await Client.PutJsonAsync($"/api/usuarios/{Guid.NewGuid()}/estado", dto);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}