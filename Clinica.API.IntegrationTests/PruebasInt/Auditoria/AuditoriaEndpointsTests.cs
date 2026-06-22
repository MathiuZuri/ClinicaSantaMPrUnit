using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Auditoria;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.PruebasInt.Auditoria;

[Collection("IntegrationTests")]
public class AuditoriaEndpointsTests : IntegrationTestBase
{
    public AuditoriaEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Auditoria_SinToken_DeberiaRetornarUnauthorized()
    {
        // Arrange
        ClearAuthorization();

        // Act
        var response = await Client.GetAsync("/api/auditoria?pagina=1&cantidadPorPagina=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Auditoria_ConAdmin_DeberiaRetornarOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/auditoria?pagina=1&cantidadPorPagina=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<AuditoriaResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Pagina.Should().Be(1);
        paginado.CantidadPorPagina.Should().Be(10);
    }

    [Fact]
    public async Task Get_Auditoria_ConFiltroTipoAccion_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Insertamos registros con diferentes tipos de acción
        await using var db = CreateDbContext();

        var adminId = db.Usuarios.First(u => u.UserName == "admin").Id;

        db.Auditorias.Add(new Domain.Entities.Auditoria
        {
            Id = Guid.NewGuid(),
            UsuarioId = adminId,
            TipoAccion = TipoAccionAuditoria.Creacion,
            Modulo = "Test",
            EntidadAfectada = "Test",
            FechaHora = DateTime.UtcNow,
            EsConsulta = false,
            FueExitoso = true
        });
        db.Auditorias.Add(new Domain.Entities.Auditoria
        {
            Id = Guid.NewGuid(),
            UsuarioId = adminId,
            TipoAccion = TipoAccionAuditoria.Edicion,
            Modulo = "Test",
            EntidadAfectada = "Test",
            FechaHora = DateTime.UtcNow.AddMinutes(-5),
            EsConsulta = false,
            FueExitoso = true
        });
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/auditoria?pagina=1&cantidadPorPagina=10&tipoAccion={(int)TipoAccionAuditoria.Creacion}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<AuditoriaResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Datos.Should().OnlyContain(x => x.TipoAccion == TipoAccionAuditoria.Creacion);
    }

    [Fact]
    public async Task Get_Auditoria_ConFiltroSoloConsultas_DeberiaFiltrarSoloConsultas()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var adminId = db.Usuarios.First(u => u.UserName == "admin").Id;

        db.Auditorias.Add(new Domain.Entities.Auditoria
        {
            Id = Guid.NewGuid(),
            UsuarioId = adminId,
            TipoAccion = TipoAccionAuditoria.Consulta,
            Modulo = "Test",
            EntidadAfectada = "Test",
            FechaHora = DateTime.UtcNow,
            EsConsulta = true,
            FueExitoso = true
        });
        db.Auditorias.Add(new Domain.Entities.Auditoria
        {
            Id = Guid.NewGuid(),
            UsuarioId = adminId,
            TipoAccion = TipoAccionAuditoria.Creacion,
            Modulo = "Test",
            EntidadAfectada = "Test",
            FechaHora = DateTime.UtcNow,
            EsConsulta = false,
            FueExitoso = true
        });
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/auditoria?pagina=1&cantidadPorPagina=10&soloConsultas=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<AuditoriaResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Datos.Should().OnlyContain(x => x.EsConsulta == true);
    }

    [Fact]
    public async Task Get_Auditoria_PorUsuario_DeberiaRetornarSoloDelUsuario()
    {
        // Arrange
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var adminId = db.Usuarios.First(u => u.UserName == "admin").Id;

        // Creamos un registro de auditoría para el admin
        db.Auditorias.Add(new Domain.Entities.Auditoria
        {
            Id = Guid.NewGuid(),
            UsuarioId = adminId,
            TipoAccion = TipoAccionAuditoria.Creacion,
            Modulo = "Pacientes",
            EntidadAfectada = "Paciente",
            FechaHora = DateTime.UtcNow,
            EsConsulta = false,
            FueExitoso = true
        });
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/auditoria/usuario/{adminId}?pagina=1&cantidadPorPagina=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<AuditoriaResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Datos.Should().NotBeEmpty();
        paginado.Datos.Should().OnlyContain(x => x.UsuarioId == adminId);
    }

    [Fact]
    public async Task Get_Auditoria_PorUsuarioInexistente_DeberiaRetornarOkConListaVacia()
    {
        // Arrange
        await LoginAsAdminAsync();

        var usuarioInexistente = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/auditoria/usuario/{usuarioInexistente}?pagina=1&cantidadPorPagina=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paginado = await response.ReadDataAsJsonAsync<PaginacionResponseDto<AuditoriaResponseDto>>();
        paginado.Should().NotBeNull();
        paginado!.Datos.Should().BeEmpty();
    }
}