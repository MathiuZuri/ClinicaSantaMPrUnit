using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authService = Substitute.For<IAuthService>();
        _controller = new AuthController(_authService);
    }

    [Fact]
    public async Task Login_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "Password123!"
        };

        var respuesta = new RespuestaInicioSesionDto
        {
            UsuarioId = Guid.NewGuid(),
            CodigoUsuario = "USR-2026-ABCDE",
            NombreCompleto = "Kevin Paricahua",
            Correo = "kevin@correo.com",
            Token = "token.jwt.prueba",
            Roles = new List<string> { "Administrador" },
            Permisos = new List<string> { "USUARIO_VER", "PACIENTE_VER" }
        };

        _authService.IniciarSesionAsync(dto).Returns(respuesta);

        // Act
        var resultado = await _controller.Login(dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Inicio de sesión correcto.");
        response.Data.Should().BeOfType<RespuestaInicioSesionDto>();

        var data = (RespuestaInicioSesionDto)response.Data!;
        data.UsuarioId.Should().Be(respuesta.UsuarioId);
        data.CodigoUsuario.Should().Be(respuesta.CodigoUsuario);
        data.NombreCompleto.Should().Be(respuesta.NombreCompleto);
        data.Correo.Should().Be(respuesta.Correo);
        data.Token.Should().Be(respuesta.Token);
        data.Roles.Should().BeEquivalentTo(respuesta.Roles);
        data.Permisos.Should().BeEquivalentTo(respuesta.Permisos);
    }

    [Fact]
    public async Task Login_DebeInvocarServicioUnaVez()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "Password123!"
        };

        var respuesta = new RespuestaInicioSesionDto
        {
            UsuarioId = Guid.NewGuid(),
            CodigoUsuario = "USR-2026-ABCDE",
            NombreCompleto = "Kevin Paricahua",
            Correo = "kevin@correo.com",
            Token = "token.jwt.prueba"
        };

        _authService.IniciarSesionAsync(dto).Returns(respuesta);

        // Act
        await _controller.Login(dto);

        // Assert
        await _authService.Received(1).IniciarSesionAsync(dto);
    }

    [Fact]
    public async Task Login_SiServicioLanzaExcepcion_DebePropagarla()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "incorrecta"
        };

        _authService
            .IniciarSesionAsync(dto)
            .Returns<Task<RespuestaInicioSesionDto>>(_ => throw new InvalidOperationException("Usuario o contraseña incorrectos."));

        // Act
        Func<Task> act = async () => await _controller.Login(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Usuario o contraseña incorrectos.");
    }
    
    [Fact]
    public async Task CambiarContrasena_DebeRetornarOk()
    {
        var dto = new CambiarContrasenaDto
        {
            ContrasenaActual = "Old",
            ContrasenaNueva = "New"
        };

        _authService.CambiarContrasenaAsync(dto).Returns(Task.CompletedTask);

        var resultado = await _controller.CambiarContrasena(dto);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Contraseña actualizada correctamente.");
        await _authService.Received(1).CambiarContrasenaAsync(dto);
    }
}