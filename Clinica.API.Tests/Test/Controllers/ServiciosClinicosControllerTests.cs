using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Servicios;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class ServiciosClinicosControllerTests
{
    private readonly IServicioClinicoService _servicioService;
    private readonly ServiciosClinicosController _controller;

    public ServiciosClinicosControllerTests()
    {
        _servicioService = Substitute.For<IServicioClinicoService>();
        _controller = new ServiciosClinicosController(_servicioService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var servicios = new List<ServicioClinicoResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto()
        };

        _servicioService.ObtenerTodosAsync().Returns(servicios);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Servicios clínicos obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<ServicioClinicoResponseDto>>();
        ((IEnumerable<ServicioClinicoResponseDto>)response.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActivos_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var servicios = new List<ServicioClinicoResponseDto> { CrearResponseDto() };

        _servicioService.ObtenerActivosAsync().Returns(servicios);

        // Act
        var resultado = await _controller.GetActivos();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Servicios clínicos activos obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<ServicioClinicoResponseDto>>();
    }

    [Fact]
    public async Task GetById_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var servicio = CrearResponseDto(id: id);

        _servicioService.ObtenerPorIdAsync(id).Returns(servicio);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Servicio clínico obtenido correctamente.");
        response.Data.Should().BeOfType<ServicioClinicoResponseDto>();
        ((ServicioClinicoResponseDto)response.Data!).Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _servicioService.ObtenerPorIdAsync(id).Returns((ServicioClinicoResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetById(id);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Servicio clínico no encontrado.");
    }

    private static ServicioClinicoResponseDto CrearResponseDto(Guid? id = null)
    {
        return new ServicioClinicoResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            CodigoServicio = "CONOBS",
            Nombre = "Consulta obstétrica",
            Descripcion = "Consulta general especializada",
            CostoBase = 80.50m,
            DuracionMinutos = 30,
            RequiereCita = true,
            GeneraHistorial = true,
            Estado = EstadoServicioClinico.Activo
        };
    }
    
    [Fact]
    public async Task GetActivos_CuandoNoHayActivos_DebeRetornarOkConListaVacia()
    {
        _servicioService.ObtenerActivosAsync().Returns(new List<ServicioClinicoResponseDto>());

        var resultado = await _controller.GetActivos();

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Exitoso.Should().BeTrue();
        var data = response.Data as IEnumerable<ServicioClinicoResponseDto>;
        data.Should().BeEmpty();
    }
}