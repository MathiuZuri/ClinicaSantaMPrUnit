using Clinica.API.Controllers;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Auditoria;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class AuditoriaControllerTests
{
    private readonly IAuditoriaService _auditoriaService;
    private readonly AuditoriaController _controller;

    public AuditoriaControllerTests()
    {
        _auditoriaService = Substitute.For<IAuditoriaService>();
        _controller = new AuditoriaController(_auditoriaService);
    }

    [Fact]
    public async Task GetAll_SinFiltros_RetornaOkConDatosPaginados()
    {
        // Arrange
        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };
        var respuesta = new PaginacionResponseDto<AuditoriaResponseDto>
        {
            Pagina = 1,
            CantidadPorPagina = 10,
            TotalRegistros = 0,
            Datos = new List<AuditoriaResponseDto>()
        };

        _auditoriaService.ObtenerTodosPaginadosAsync(request, null, null)
            .Returns(respuesta);

        // Act
        var result = await _controller.GetAll(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<Models.ApiResponse<object>>().Subject;
        apiResponse.Exitoso.Should().BeTrue();
        await _auditoriaService.Received(1).ObtenerTodosPaginadosAsync(request, null, null);
    }

    [Fact]
    public async Task GetAll_ConFiltros_RetornaOkConFiltrosAplicados()
    {
        // Arrange
        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 5 };
        var tipo = TipoAccionAuditoria.Edicion;
        var soloConsultas = false;

        _auditoriaService.ObtenerTodosPaginadosAsync(request, tipo, soloConsultas)
            .Returns(new PaginacionResponseDto<AuditoriaResponseDto>());

        // Act
        var result = await _controller.GetAll(request, tipo, soloConsultas);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        await _auditoriaService.Received(1).ObtenerTodosPaginadosAsync(request, tipo, soloConsultas);
    }

    [Fact]
    public async Task GetByUsuario_RetornaOkConDatosPaginados()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };

        _auditoriaService.ObtenerPorUsuarioPaginadosAsync(usuarioId, request, null, null)
            .Returns(new PaginacionResponseDto<AuditoriaResponseDto>());

        // Act
        var result = await _controller.GetByUsuario(usuarioId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        await _auditoriaService.Received(1).ObtenerPorUsuarioPaginadosAsync(usuarioId, request, null, null);
    }
}