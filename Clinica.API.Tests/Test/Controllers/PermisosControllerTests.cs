using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Permisos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class PermisosControllerTests
{
    private readonly IPermisoService _permisoService;
    private readonly PermisosController _controller;

    public PermisosControllerTests()
    {
        _permisoService = Substitute.For<IPermisoService>();
        _controller = new PermisosController(_permisoService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var permisos = new List<PermisoResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto(
                codigo: "PACIENTE_CREAR",
                nombre: "Crear pacientes",
                modulo: "Pacientes")
        };

        _permisoService.ObtenerTodosAsync().Returns(permisos);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Permisos obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<PermisoResponseDto>>();

        var data = ((IEnumerable<PermisoResponseDto>)response.Data!).ToList();
        data.Should().HaveCount(2);
        data[0].Codigo.Should().Be("PACIENTE_VER");
        data[1].Codigo.Should().Be("PACIENTE_CREAR");
    }

    [Fact]
    public async Task GetAll_SiNoHayPermisos_DebeRetornarOkConListaVacia()
    {
        // Arrange
        _permisoService.ObtenerTodosAsync().Returns(Enumerable.Empty<PermisoResponseDto>());

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Permisos obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<PermisoResponseDto>>();

        var data = ((IEnumerable<PermisoResponseDto>)response.Data!).ToList();
        data.Should().BeEmpty();
    }

    private static PermisoResponseDto CrearResponseDto(
        string codigo = "PACIENTE_VER",
        string nombre = "Ver pacientes",
        string modulo = "Pacientes")
    {
        return new PermisoResponseDto
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nombre = nombre,
            Modulo = modulo,
            Descripcion = "Descripción de prueba",
            Activo = true
        };
    }
}