using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Historiales;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class HistorialesControllerTests
{
    private readonly IHistorialClinicoService _historialService;
    private readonly HistorialesController _controller;

    public HistorialesControllerTests()
    {
        _historialService = Substitute.For<IHistorialClinicoService>();
        _controller = new HistorialesController(_historialService);
    }

    [Fact]
    public async Task GetByPaciente_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var historial = CrearResponseDto();

        _historialService.ObtenerPorPacienteAsync(pacienteId).Returns(historial);

        // Act
        var resultado = await _controller.GetByPaciente(pacienteId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Historial clínico obtenido correctamente.");
        response.Data.Should().BeOfType<HistorialClinicoResponseDto>();

        var data = (HistorialClinicoResponseDto)response.Data!;
        data.Id.Should().Be(historial.Id);
        data.CodigoHistorial.Should().Be(historial.CodigoHistorial);
        data.PacienteId.Should().Be(historial.PacienteId);
        data.PacienteNombre.Should().Be(historial.PacienteNombre);
        data.PacienteDni.Should().Be(historial.PacienteDni);
        data.Estado.Should().Be(historial.Estado);
        data.Detalles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByPaciente_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        _historialService.ObtenerPorPacienteAsync(pacienteId).Returns((HistorialClinicoResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetByPaciente(pacienteId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Historial clínico no encontrado.");
    }

    [Fact]
    public async Task GetConDetalles_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var historial = CrearResponseDto(id: historialId);

        _historialService.ObtenerConDetallesAsync(historialId).Returns(historial);

        // Act
        var resultado = await _controller.GetConDetalles(historialId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Historial clínico con detalles obtenido correctamente.");
        response.Data.Should().BeOfType<HistorialClinicoResponseDto>();

        var data = (HistorialClinicoResponseDto)response.Data!;
        data.Id.Should().Be(historialId);
        data.Detalles.Should().HaveCount(1);
        data.Detalles[0].CodigoDetalle.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetConDetalles_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        _historialService.ObtenerConDetallesAsync(historialId).Returns((HistorialClinicoResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetConDetalles(historialId);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Historial clínico no encontrado.");
    }

    [Fact]
    public async Task GetConDetalles_DebeRetornarDetalleConUsuarioNombreNull_CuandoCorresponde()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var historial = CrearResponseDto(id: historialId);
        historial.Detalles[0].UsuarioId = null;
        historial.Detalles[0].UsuarioNombre = null;

        _historialService.ObtenerConDetallesAsync(historialId).Returns(historial);

        // Act
        var resultado = await _controller.GetConDetalles(historialId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        var data = (HistorialClinicoResponseDto)response.Data!;

        data.Detalles.Should().HaveCount(1);
        data.Detalles[0].UsuarioId.Should().BeNull();
        data.Detalles[0].UsuarioNombre.Should().BeNull();
    }

    [Fact]
    public async Task GetByPaciente_DebeRetornarHistorialConListaDeDetallesVacia()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var historial = CrearResponseDto();
        historial.Detalles = new List<HistorialDetalleResponseDto>();

        _historialService.ObtenerPorPacienteAsync(pacienteId).Returns(historial);

        // Act
        var resultado = await _controller.GetByPaciente(pacienteId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        var data = (HistorialClinicoResponseDto)response.Data!;

        data.Detalles.Should().NotBeNull().And.BeEmpty();
    }

    private static HistorialClinicoResponseDto CrearResponseDto(Guid? id = null)
    {
        return new HistorialClinicoResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            CodigoHistorial = "ABCDE-2026-12345678",
            PacienteId = Guid.NewGuid(),
            PacienteNombre = "Ana Quispe",
            PacienteDni = "12345678",
            FechaApertura = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc),
            Estado = EstadoHistorialClinico.Activo,
            Detalles = new List<HistorialDetalleResponseDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CodigoDetalle = "ABCDE-REG-2026-12345678",
                    HistorialClinicoId = Guid.NewGuid(),
                    TipoMovimiento = TipoMovimientoHistorial.AperturaHistorial,
                    CitaId = Guid.NewGuid(),
                    AtencionId = Guid.NewGuid(),
                    PagoId = Guid.NewGuid(),
                    Titulo = "Apertura de historial clínico",
                    Descripcion = "Se aperturó el historial clínico del paciente.",
                    FechaRegistro = new DateTime(2026, 1, 10, 9, 5, 0, DateTimeKind.Utc),
                    UsuarioId = Guid.NewGuid(),
                    UsuarioNombre = "Carlos Mamani"
                }
            }
        };
    }
}