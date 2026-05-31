using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Roles;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class RolesControllerTests
{
    private readonly IRolService _rolService;
    private readonly RolesController _controller;

    public RolesControllerTests()
    {
        _rolService = Substitute.For<IRolService>();
        _controller = new RolesController(_rolService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var roles = new List<RolResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto()
        };

        _rolService.ObtenerTodosAsync().Returns(roles);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Roles obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<RolResponseDto>>();
        ((IEnumerable<RolResponseDto>)response.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rol = CrearResponseDto(id: id);

        _rolService.ObtenerPorIdAsync(id).Returns(rol);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Rol obtenido correctamente.");
        response.Data.Should().BeOfType<RolResponseDto>();
        ((RolResponseDto)response.Data!).Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _rolService.ObtenerPorIdAsync(id).Returns((RolResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetById(id);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Rol no encontrado.");
    }

    [Fact]
    public async Task Create_DebeRetornarCreatedAtActionConApiResponse()
    {
        // Arrange
        var dto = new CrearRolDto
        {
            Nombre = "Recepcionista",
            Descripcion = "Rol de recepción"
        };

        var nuevoId = Guid.NewGuid();
        _rolService.CrearAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;

        createdResult.ActionName.Should().Be(nameof(RolesController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(nuevoId);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(201);
        response.Mensaje.Should().Be("Rol creado correctamente.");
    }

    [Fact]
    public async Task Update_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new EditarRolDto
        {
            Nombre = "Caja",
            Descripcion = "Rol actualizado",
            Activo = false
        };

        _rolService.ActualizarAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.Update(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Rol actualizado correctamente.");

        await _rolService.Received(1).ActualizarAsync(id, dto);
    }

    [Fact]
    public async Task AssignPermissions_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var dto = new AsignarPermisosRolDto
        {
            RolId = Guid.NewGuid(),
            PermisosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        _rolService.AsignarPermisosAsync(dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.AssignPermissions(dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Permisos asignados correctamente.");

        await _rolService.Received(1).AsignarPermisosAsync(dto);
    }

    private static RolResponseDto CrearResponseDto(Guid? id = null)
    {
        return new RolResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            Nombre = "Administrador",
            Descripcion = "Rol principal",
            EsSistema = false,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
    }
}