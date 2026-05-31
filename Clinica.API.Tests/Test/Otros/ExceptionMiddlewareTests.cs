using System.Text.Json;
using Clinica.API.Middlewares;
using Clinica.API.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SiNoHayExcepcion_DebeContinuarPipeline()
    {
        // Arrange
        var wasCalled = false;

        RequestDelegate next = _ =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var environment = Substitute.For<IWebHostEnvironment>();
        var middleware = new ExceptionMiddleware(next, environment);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        wasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SiHayKeyNotFoundException_DebeRetornar404()
    {
        // Arrange
        RequestDelegate next = _ => throw new KeyNotFoundException("No encontrado");
        var environment = Substitute.For<IWebHostEnvironment>();
        var middleware = new ExceptionMiddleware(next, environment);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var response = await LeerRespuestaAsync(context);
        response.Mensaje.Should().Be("No encontrado");
        response.Codigo.Should().Be(StatusCodes.Status404NotFound);
        response.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_SiHayInvalidOperationException_DebeRetornar400()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Operación inválida");
        var environment = Substitute.For<IWebHostEnvironment>();
        var middleware = new ExceptionMiddleware(next, environment);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var response = await LeerRespuestaAsync(context);
        response.Mensaje.Should().Be("Operación inválida");
        response.Codigo.Should().Be(StatusCodes.Status400BadRequest);
        response.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_SiHayUnauthorizedAccessException_DebeRetornar401()
    {
        // Arrange
        RequestDelegate next = _ => throw new UnauthorizedAccessException("No autorizado");
        var environment = Substitute.For<IWebHostEnvironment>();
        var middleware = new ExceptionMiddleware(next, environment);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var response = await LeerRespuestaAsync(context);
        response.Mensaje.Should().Be("No autorizado");
        response.Codigo.Should().Be(StatusCodes.Status401Unauthorized);
        response.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_SiHayExcepcionGeneralEnDevelopment_DebeRetornarMensajeReal()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Error real de desarrollo");
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        var middleware = new ExceptionMiddleware(next, environment);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var response = await LeerRespuestaAsync(context);
        response.Mensaje.Should().Be("Error real de desarrollo");
        response.Codigo.Should().Be(StatusCodes.Status500InternalServerError);
        response.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_SiHayExcepcionGeneralEnProduccion_DebeRetornarMensajeGenerico()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Error interno sensible");
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns("Production");

        var middleware = new ExceptionMiddleware(next, environment);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var response = await LeerRespuestaAsync(context);
        response.Mensaje.Should().Be("Ocurrió un error interno en el servidor.");
        response.Mensaje.Should().NotBe("Error interno sensible");
        response.Codigo.Should().Be(StatusCodes.Status500InternalServerError);
        response.Exitoso.Should().BeFalse();
    }

    private static async Task<ApiResponse<object>> LeerRespuestaAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        var response = JsonSerializer.Deserialize<ApiResponse<object>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        response.Should().NotBeNull();
        return response!;
    }
}