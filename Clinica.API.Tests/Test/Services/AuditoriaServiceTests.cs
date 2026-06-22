using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Auditoria;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Clinica.Infrastructure.Repositories;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class AuditoriaServiceTests
{
    private IAuditoriaRepository CrearRepositorio(ApplicationDbContext db)
    {
        return new AuditoriaRepository(db);
    }
    
    private ApplicationDbContext CrearDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private async Task SembrarDatosAsync(ApplicationDbContext db)
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombres = "Ana",
            Apellidos = "Prueba",
            UserName = "ana",
            Correo = "ana@test.com"
        };

        db.Usuarios.Add(usuario);

        var registros = new List<Auditoria>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                Usuario = usuario,
                TipoAccion = TipoAccionAuditoria.Creacion,
                Modulo = "Pacientes",
                EntidadAfectada = "Paciente",
                FechaHora = new DateTime(2026, 1, 10),
                EsConsulta = false,
                FueExitoso = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                Usuario = usuario,
                TipoAccion = TipoAccionAuditoria.Consulta,
                Modulo = "Citas",
                EntidadAfectada = "Cita",
                FechaHora = new DateTime(2026, 1, 11),
                EsConsulta = true,
                FueExitoso = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                Usuario = usuario,
                TipoAccion = TipoAccionAuditoria.Edicion,
                Modulo = "Pacientes",
                EntidadAfectada = "Paciente",
                FechaHora = new DateTime(2026, 1, 12),
                EsConsulta = false,
                FueExitoso = false
            }
        };

        db.Auditorias.AddRange(registros);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task ObtenerTodosPaginadosAsync_SinFiltros_RetornaTodosLosRegistros()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var service = new AuditoriaService(null!, db);

        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };

        // Act
        var resultado = await service.ObtenerTodosPaginadosAsync(request);

        // Assert
        resultado.TotalRegistros.Should().Be(3);
        resultado.TotalPaginas.Should().Be(1);
        resultado.Datos.Should().HaveCount(3);
        resultado.Datos.Should().BeInDescendingOrder(x => x.FechaHora);
    }

    [Fact]
    public async Task ObtenerTodosPaginadosAsync_FiltroPorTipoAccion_RetornaSoloEseTipo()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var service = new AuditoriaService(null!, db);

        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };

        // Act
        var resultado = await service.ObtenerTodosPaginadosAsync(request, tipoAccion: TipoAccionAuditoria.Creacion);

        // Assert
        resultado.TotalRegistros.Should().Be(1);
        resultado.TotalPaginas.Should().Be(1);
        resultado.Datos.Should().ContainSingle(x => x.TipoAccion == TipoAccionAuditoria.Creacion);
    }

    [Fact]
    public async Task ObtenerTodosPaginadosAsync_FiltroSoloConsultas_RetornaSoloConsultas()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var service = new AuditoriaService(null!, db);

        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };

        // Act
        var resultado = await service.ObtenerTodosPaginadosAsync(request, soloConsultas: true);

        // Assert
        resultado.TotalRegistros.Should().Be(1);
        resultado.TotalPaginas.Should().Be(1);
        resultado.Datos.Should().ContainSingle(x => x.EsConsulta == true);
    }

    [Fact]
    public async Task ObtenerTodosPaginadosAsync_Paginacion_RetornaPaginaCorrecta()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var service = new AuditoriaService(null!, db);

        var request = new PaginacionRequestDto { Pagina = 2, CantidadPorPagina = 1 };

        // Act
        var resultado = await service.ObtenerTodosPaginadosAsync(request);

        // Assert
        resultado.TotalRegistros.Should().Be(3);
        resultado.Datos.Should().HaveCount(1);
        resultado.Pagina.Should().Be(2);
    }

    [Fact]
    public async Task ObtenerPorUsuarioPaginadosAsync_SinFiltros_RetornaRegistrosDelUsuario()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var service = new AuditoriaService(null!, db);
        var usuarioId = db.Auditorias.First().UsuarioId!.Value;

        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };

        // Act
        var resultado = await service.ObtenerPorUsuarioPaginadosAsync(usuarioId, request);

        // Assert
        resultado.TotalRegistros.Should().Be(3);
        resultado.Datos.Should().OnlyContain(x => x.UsuarioId == usuarioId);
    }

    [Fact]
    public async Task ObtenerPorUsuarioPaginadosAsync_UsuarioSinRegistros_RetornaVacio()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var service = new AuditoriaService(null!, db);
        var usuarioIdInexistente = Guid.NewGuid();

        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };

        // Act
        var resultado = await service.ObtenerPorUsuarioPaginadosAsync(usuarioIdInexistente, request);

        // Assert
        resultado.TotalRegistros.Should().Be(0);
        resultado.Datos.Should().BeEmpty();
    }
    [Fact]
    public async Task ObtenerTodosAsync_SinPaginacion_RetornaTodosLosRegistros()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var repo = CrearRepositorio(db);
        var service = new AuditoriaService(repo, db);

        // Act
        var resultado = (await service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(3);
        resultado.Should().BeInDescendingOrder(x => x.FechaHora);
        resultado[0].UsuarioNombre.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerPorUsuarioAsync_SinPaginacion_RetornaSoloDelUsuario()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var usuarioId = db.Auditorias.First().UsuarioId!.Value;
        var repo = CrearRepositorio(db);
        var service = new AuditoriaService(repo, db);

        // Act
        var resultado = (await service.ObtenerPorUsuarioAsync(usuarioId)).ToList();

        // Assert
        resultado.Should().HaveCount(3);
        resultado.Should().OnlyContain(x => x.UsuarioId == usuarioId);
    }

    [Fact]
    public async Task ObtenerPorUsuarioAsync_UsuarioSinRegistros_RetornaListaVacia()
    {
        // Arrange
        await using var db = CrearDbContext();
        await SembrarDatosAsync(db);
        var repo = CrearRepositorio(db);
        var service = new AuditoriaService(repo, db);
        var usuarioInexistente = Guid.NewGuid();

        // Act
        var resultado = (await service.ObtenerPorUsuarioAsync(usuarioInexistente)).ToList();

        // Assert
        resultado.Should().BeEmpty();
    }
}