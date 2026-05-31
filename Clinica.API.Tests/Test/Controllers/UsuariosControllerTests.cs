using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Usuarios;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class UsuariosControllerTests
{
    private readonly IUsuarioService _usuarioService;
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        _usuarioService = Substitute.For<IUsuarioService>();
        _controller = new UsuariosController(_usuarioService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var usuarios = new List<UsuarioResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto()
        };

        _usuarioService.ObtenerTodosAsync().Returns(usuarios);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Usuarios obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<UsuarioResponseDto>>();
        ((IEnumerable<UsuarioResponseDto>)response.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuario = CrearResponseDto(id: id);

        _usuarioService.ObtenerPorIdAsync(id).Returns(usuario);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Usuario obtenido correctamente.");
        response.Data.Should().BeOfType<UsuarioResponseDto>();
        ((UsuarioResponseDto)response.Data!).Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _usuarioService.ObtenerPorIdAsync(id).Returns((UsuarioResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetById(id);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Usuario no encontrado.");
    }

    [Fact]
    public async Task Create_DebeRetornarCreatedAtActionConApiResponse()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Nombres = "Kevin",
            Apellidos = "Paricahua",
            UserName = "kevin.paricahua",
            Correo = "kevin@correo.com",
            Password = "Password123!"
        };

        var nuevoId = Guid.NewGuid();
        _usuarioService.CrearAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;

        createdResult.ActionName.Should().Be(nameof(UsuariosController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(nuevoId);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(201);
        response.Mensaje.Should().Be("Usuario creado correctamente.");
    }

    [Fact]
    public async Task Update_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new EditarUsuarioDto
        {
            Nombres = "Carlos",
            Apellidos = "Mamani",
            UserName = "carlos.mamani",
            Correo = "carlos@correo.com"
        };

        _usuarioService.ActualizarAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.Update(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Usuario actualizado correctamente.");

        await _usuarioService.Received(1).ActualizarAsync(id, dto);
    }

    [Fact]
    public async Task AssignRole_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = Guid.NewGuid(),
            RolId = Guid.NewGuid()
        };

        _usuarioService.AsignarRolAsync(dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.AssignRole(dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Rol asignado correctamente.");

        await _usuarioService.Received(1).AsignarRolAsync(dto);
    }

    private static UsuarioResponseDto CrearResponseDto(Guid? id = null)
    {
        return new UsuarioResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            CodigoUsuario = "USR-2026-ABCDE",
            Nombres = "Kevin",
            Apellidos = "Paricahua",
            UserName = "kevin.paricahua",
            Correo = "kevin@correo.com",
            Estado = EstadoUsuario.Activo,
            FechaRegistro = DateTime.UtcNow,
            UltimoAcceso = DateTime.UtcNow.AddMinutes(-10)
        };
    }
    
    [Fact]
    public async Task CambiarEstado_DebeRetornarOkConApiResponse()
    {
        var id = Guid.NewGuid();
        var dto = new CambiarEstadoUsuarioDto { Estado = EstadoUsuario.Inactivo };

        _usuarioService.CambiarEstadoAsync(id, dto).Returns(Task.CompletedTask);

        var resultado = await _controller.CambiarEstado(id, dto);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Estado del usuario actualizado correctamente.");

        await _usuarioService.Received(1).CambiarEstadoAsync(id, dto);
    }
}