using System.Net;
using Clinica.API.Authorization;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Roles;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.IntegrationTests.PruebasInt.Roles;

[Collection("IntegrationTests")]
public class RolesEndpointsTests : IntegrationTestBase
{
    public RolesEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Roles_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Roles_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_Roles_DeberiaIncluirRolesDelSeeder()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var roles = await response.ReadDataAsJsonAsync<List<RolResponseDto>>();

        roles.Should().NotBeNull();
        roles.Should().NotBeEmpty();

        roles!.Should().Contain(x => x.Nombre == "Administrador" && x.EsSistema);
        roles.Should().Contain(x => x.Nombre == "Recepcionista" && x.EsSistema);
        roles.Should().Contain(x => x.Nombre == "Doctor" && x.EsSistema);
        roles.Should().Contain(x => x.Nombre == "Caja" && x.EsSistema);
    }

    [Fact]
    public async Task Post_Roles_Valido_DeberiaCrearRol()
    {
        // Arrange
        await LoginAsAdminAsync();

        var nombreRol = $"Rol Test {Guid.NewGuid():N}"[..30];

        var dto = new CrearRolDto
        {
            Nombre = nombreRol,
            Descripcion = "Rol creado desde prueba de integración."
        };

        // Act
        var postResponse = await Client.PostJsonAsync("/api/roles", dto);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(postResponse);

        var data = await JsonTestHelper.ReadDataAsync(postResponse);

        data.TryGetProperty("id", out var idProperty)
            .Should()
            .BeTrue("la respuesta de creación debe devolver el id del rol");

        var rolId = idProperty.GetGuid();

        var getResponse = await Client.GetAsync($"/api/roles/{rolId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var rol = await getResponse.ReadDataAsJsonAsync<RolResponseDto>();

        rol.Should().NotBeNull();
        rol!.Id.Should().Be(rolId);
        rol.Nombre.Should().Be(dto.Nombre);
        rol.Descripcion.Should().Be(dto.Descripcion);
        rol.Activo.Should().BeTrue();
        rol.EsSistema.Should().BeFalse();
    }

    [Fact]
    public async Task Get_Roles_PorIdExistente_DeberiaRetornarRol()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rolCreado = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Consulta Test",
            permisos: new[] { PermisosPolicies.PacienteVer }
        );

        // Act
        var response = await Client.GetAsync($"/api/roles/{rolCreado.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var rol = await response.ReadDataAsJsonAsync<RolResponseDto>();

        rol.Should().NotBeNull();
        rol!.Id.Should().Be(rolCreado.Id);
        rol.Nombre.Should().Be(rolCreado.Nombre);
        rol.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Roles_PorIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var idInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/roles/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Roles_NombreInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearRolDto
        {
            Nombre = "AB",
            Descripcion = "Nombre demasiado corto."
        };

        // Act
        var response = await Client.PostJsonAsync("/api/roles", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Roles_DescripcionDemasiadoLarga_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new CrearRolDto
        {
            Nombre = "Rol Valido",
            Descripcion = new string('A', 251)
        };

        // Act
        var response = await Client.PostJsonAsync("/api/roles", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Roles_Valido_DeberiaActualizarRol()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rolCreado = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Actualizar Test",
            permisos: null
        );

        var dto = new EditarRolDto
        {
            Nombre = $"Rol Editado {Guid.NewGuid():N}"[..30],
            Descripcion = "Descripción actualizada desde integración.",
            Activo = true
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/roles/{rolCreado.Id}",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/roles/{rolCreado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var rol = await getResponse.ReadDataAsJsonAsync<RolResponseDto>();

        rol.Should().NotBeNull();
        rol!.Id.Should().Be(rolCreado.Id);
        rol.Nombre.Should().Be(dto.Nombre);
        rol.Descripcion.Should().Be(dto.Descripcion);
        rol.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Put_Roles_Inactivar_DeberiaActualizarActivoFalse()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rolCreado = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Inactivar Test",
            permisos: null
        );

        var dto = new EditarRolDto
        {
            Nombre = rolCreado.Nombre,
            Descripcion = rolCreado.Descripcion,
            Activo = false
        };

        // Act
        var putResponse = await Client.PutJsonAsync(
            $"/api/roles/{rolCreado.Id}",
            dto
        );

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(putResponse);

        var getResponse = await Client.GetAsync($"/api/roles/{rolCreado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var rol = await getResponse.ReadDataAsJsonAsync<RolResponseDto>();

        rol.Should().NotBeNull();
        rol!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task Put_Roles_IdInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var dto = new EditarRolDto
        {
            Nombre = "Rol Inexistente",
            Descripcion = "No debería existir.",
            Activo = true
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/roles/{Guid.NewGuid()}",
            dto
        );

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    public async Task Put_Roles_NombreInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rolCreado = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Nombre Invalido Test",
            permisos: null
        );

        var dto = new EditarRolDto
        {
            Nombre = "AB",
            Descripcion = "Nombre inválido.",
            Activo = true
        };

        // Act
        var response = await Client.PutJsonAsync(
            $"/api/roles/{rolCreado.Id}",
            dto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Roles_AsignarPermisosValido_DeberiaAsignarPermisos()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rol = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Permisos Test",
            permisos: null
        );

        var permiso = await db.Permisos
            .FirstAsync(x => x.Codigo == PermisosPolicies.PacienteVer);

        var dto = new AsignarPermisosRolDto
        {
            RolId = rol.Id,
            PermisosIds = new List<Guid> { permiso.Id }
        };

        // Act
        var response = await Client.PostJsonAsync("/api/roles/asignar-permisos", dto);

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"el backend respondió: {body}"
        );
        await JsonTestHelper.AssertSuccessAsync(response);

        var existeRelacion = await db.RolPermisos.AnyAsync(x =>
            x.RolId == rol.Id &&
            x.PermisoId == permiso.Id);

        existeRelacion.Should().BeTrue();
    }

    [Fact]
    public async Task Post_Roles_AsignarPermisosRolVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var permiso = await db.Permisos
            .FirstAsync(x => x.Codigo == PermisosPolicies.PacienteVer);

        var dto = new AsignarPermisosRolDto
        {
            RolId = Guid.Empty,
            PermisosIds = new List<Guid> { permiso.Id }
        };

        // Act
        var response = await Client.PostJsonAsync("/api/roles/asignar-permisos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Roles_AsignarPermisosListaVacia_DeberiaRetornarBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rol = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Lista Vacia Test",
            permisos: null
        );

        var dto = new AsignarPermisosRolDto
        {
            RolId = rol.Id,
            PermisosIds = new List<Guid>()
        };

        // Act
        var response = await Client.PostJsonAsync("/api/roles/asignar-permisos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Roles_AsignarPermisosRolInexistente_DeberiaRetornarBadRequestONotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var permiso = await db.Permisos
            .FirstAsync(x => x.Codigo == PermisosPolicies.PacienteVer);

        var dto = new AsignarPermisosRolDto
        {
            RolId = Guid.NewGuid(),
            PermisosIds = new List<Guid> { permiso.Id }
        };

        // Act
        var response = await Client.PostJsonAsync("/api/roles/asignar-permisos", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task Post_Roles_AsignarPermisosPermisoInexistente_DeberiaRetornarNotFoundOBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var rol = await TestDataSeeder.CrearRolAsync(
            db,
            nombre: "Rol Permiso Inexistente Test",
            permisos: null
        );

        var permisoInexistenteId = Guid.NewGuid();

        var dto = new AsignarPermisosRolDto
        {
            RolId = rol.Id,
            PermisosIds = new List<Guid> { permisoInexistenteId }
        };

        // Act
        var response = await Client.PostJsonAsync("/api/roles/asignar-permisos", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );

        var existeRelacionInvalida = await db.RolPermisos.AnyAsync(x =>
            x.RolId == rol.Id &&
            x.PermisoId == permisoInexistenteId);

        existeRelacionInvalida.Should().BeFalse(
            "no debería crearse una relación con un permiso inexistente"
        );
    }
}