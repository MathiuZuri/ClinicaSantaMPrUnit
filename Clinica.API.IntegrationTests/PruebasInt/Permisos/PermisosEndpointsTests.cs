using System.Net;
using Clinica.API.Authorization;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Permisos;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Permisos;

[Collection("IntegrationTests")]
public class PermisosEndpointsTests : IntegrationTestBase
{
    public PermisosEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Permisos_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Permisos_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_Permisos_DeberiaRetornarListaNoVacia()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var permisos = await response.ReadDataAsJsonAsync<List<PermisoResponseDto>>();

        permisos.Should().NotBeNull();
        permisos.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Get_Permisos_DeberiaIncluirPermisosBaseDelSeeder()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var permisos = await response.ReadDataAsJsonAsync<List<PermisoResponseDto>>();

        permisos.Should().NotBeNull();
        permisos.Should().NotBeEmpty();

        permisos!.Should().Contain(x =>
            x.Codigo == PermisosPolicies.PacienteVer &&
            x.Nombre == "Ver pacientes" &&
            x.Modulo == "Pacientes" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.CitaVer &&
            x.Nombre == "Ver citas" &&
            x.Modulo == "Citas" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.DoctorVer &&
            x.Nombre == "Ver doctores" &&
            x.Modulo == "Doctores" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.RolVer &&
            x.Nombre == "Ver roles" &&
            x.Modulo == "Roles" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.PermisoVer &&
            x.Nombre == "Ver permisos" &&
            x.Modulo == "Permisos" &&
            x.Activo);
    }

    [Fact]
    public async Task Get_Permisos_DeberiaIncluirPermisosDeFinanzasYComprobantes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var permisos = await response.ReadDataAsJsonAsync<List<PermisoResponseDto>>();

        permisos.Should().NotBeNull();

        permisos!.Should().Contain(x =>
            x.Codigo == PermisosPolicies.FinanzasVer &&
            x.Modulo == "Finanzas" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.FinanzasExportar &&
            x.Modulo == "Finanzas" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.FinanzasAjustar &&
            x.Modulo == "Finanzas" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.ComprobanteVer &&
            x.Modulo == "Comprobantes" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.ComprobanteEmitir &&
            x.Modulo == "Comprobantes" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.ComprobanteAnular &&
            x.Modulo == "Comprobantes" &&
            x.Activo);

        permisos.Should().Contain(x =>
            x.Codigo == PermisosPolicies.ComprobanteImprimir &&
            x.Modulo == "Comprobantes" &&
            x.Activo);
    }

    [Fact]
    public async Task Get_Permisos_DeberiaIncluirTodosLosCodigosDePermisosPolicies()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var permisos = await response.ReadDataAsJsonAsync<List<PermisoResponseDto>>();

        permisos.Should().NotBeNull();

        var codigosRetornados = permisos!
            .Select(x => x.Codigo)
            .Distinct()
            .ToList();

        codigosRetornados.Should().Contain(PermisosPolicies.Todos);
    }

    [Fact]
    public async Task Get_Permisos_NoDeberiaRetornarCodigosDuplicados()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var permisos = await response.ReadDataAsJsonAsync<List<PermisoResponseDto>>();

        permisos.Should().NotBeNull();

        var codigosDuplicados = permisos!
            .GroupBy(x => x.Codigo)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        codigosDuplicados.Should().BeEmpty(
            "cada permiso debe tener un código único dentro del sistema"
        );
    }

    [Fact]
    public async Task Get_Permisos_DeberiaRetornarPermisosActivos()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var permisos = await response.ReadDataAsJsonAsync<List<PermisoResponseDto>>();

        permisos.Should().NotBeNull();
        permisos.Should().NotBeEmpty();

        permisos!.Should().OnlyContain(x => x.Activo);
    }
}