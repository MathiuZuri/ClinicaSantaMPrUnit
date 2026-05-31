using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Servicios;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.ServiciosClinicos;

[Collection("IntegrationTests")]
public class ServiciosClinicosEndpointsTests : IntegrationTestBase
{
    public ServiciosClinicosEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_ServiciosClinicos_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/serviciosclinicos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ServiciosClinicos_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/serviciosclinicos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_ServiciosClinicosActivos_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/serviciosclinicos/activos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    [Fact]
    public async Task Get_ServiciosClinicos_DeberiaIncluirServiciosDelSeeder()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/serviciosclinicos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var servicios = await response.ReadDataAsJsonAsync<List<ServicioClinicoResponseDto>>();

        servicios.Should().NotBeNull();
        servicios.Should().NotBeEmpty();

        servicios!.Should().Contain(x =>
            x.CodigoServicio == "ATEGEN" &&
            x.Nombre == "Atención genérica");

        servicios.Should().Contain(x =>
            x.CodigoServicio == "CONOBS" &&
            x.Nombre == "Consulta obstétrica");
    }

    [Fact]
    public async Task Get_ServiciosClinicosActivos_DeberiaRetornarSoloActivos()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/serviciosclinicos/activos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var servicios = await response.ReadDataAsJsonAsync<List<ServicioClinicoResponseDto>>();

        servicios.Should().NotBeNull();
        servicios.Should().NotBeEmpty();

        servicios!.Should().OnlyContain(x => x.Estado == EstadoServicioClinico.Activo);
    }

    [Fact]
    public async Task Get_ServiciosClinicos_PorIdExistente_DeberiaRetornarServicio()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var servicioCreado = await TestDataSeeder.ObtenerOCrearServicioClinicoAsync(
            db,
            codigoServicio: $"SRV{Random.Shared.Next(1000, 9999)}",
            nombre: "Servicio de prueba",
            costoBase: 85,
            duracionMinutos: 45
        );

        // Act
        var response = await Client.GetAsync($"/api/serviciosclinicos/{servicioCreado.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var servicio = await response.ReadDataAsJsonAsync<ServicioClinicoResponseDto>();

        servicio.Should().NotBeNull();
        servicio!.Id.Should().Be(servicioCreado.Id);
        servicio.CodigoServicio.Should().Be(servicioCreado.CodigoServicio);
        servicio.Nombre.Should().Be(servicioCreado.Nombre);
        servicio.CostoBase.Should().Be(servicioCreado.CostoBase);
        servicio.DuracionMinutos.Should().Be(servicioCreado.DuracionMinutos);
        servicio.Estado.Should().Be(EstadoServicioClinico.Activo);
    }

    [Fact]
    public async Task Get_ServiciosClinicos_PorIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var idInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/serviciosclinicos/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }
    
    [Fact]
    public async Task Get_ServiciosClinicosActivos_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/serviciosclinicos/activos");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task Get_ServiciosClinicosActivos_NoDebeIncluirInactivos()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        // Insertar un servicio inactivo directamente
        var inactivo = new ServicioClinico
        {
            Id = Guid.NewGuid(),
            CodigoServicio = "INACTIVO",
            Nombre = "Servicio Inactivo",
            Estado = EstadoServicioClinico.Inactivo
        };
        db.ServiciosClinicos.Add(inactivo);
        await db.SaveChangesAsync();

        var response = await Client.GetAsync("/api/serviciosclinicos/activos");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var servicios = await response.ReadDataAsJsonAsync<List<ServicioClinicoResponseDto>>();
        servicios.Should().NotContain(s => s.Id == inactivo.Id);
    }
}