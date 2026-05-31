using System.Security.Claims;
using Clinica.API.Services.Imp;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class UsuarioActualServiceTests
{
    [Fact]
    public void ObtenerUsuarioId_SiNoHayUsuarioAutenticado_DebeLanzarUnauthorizedAccessException()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        Action act = () => service.ObtenerUsuarioId();

        // Assert
        act.Should()
            .Throw<UnauthorizedAccessException>()
            .WithMessage("No se pudo identificar al usuario autenticado.");
    }

    [Fact]
    public void ObtenerUsuarioIdOpcional_SiNoHayHttpContext_DebeRetornarNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public void ObtenerUsuarioIdOpcional_SiUsuarioNoAutenticado_DebeRetornarNull()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public void ObtenerUsuarioIdOpcional_SiExisteClaimNameIdentifier_DebeRetornarGuid()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
        ], "TestAuth");

        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().Be(usuarioId);
    }

    [Fact]
    public void ObtenerUsuarioIdOpcional_SiExisteClaimSub_DebeRetornarGuid()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", usuarioId.ToString())
        ], "TestAuth");

        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().Be(usuarioId);
    }

    [Fact]
    public void ObtenerUsuarioIdOpcional_SiExisteClaimUsuarioId_DebeRetornarGuid()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
        [
            new Claim("usuarioId", usuarioId.ToString())
        ], "TestAuth");

        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().Be(usuarioId);
    }

    [Fact]
    public void ObtenerUsuarioIdOpcional_SiExisteClaimUsuarioIdMayus_DebeRetornarGuid()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
        [
            new Claim("UsuarioId", usuarioId.ToString())
        ], "TestAuth");

        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().Be(usuarioId);
    }

    [Fact]
    public void ObtenerUsuarioIdOpcional_SiClaimNoEsGuidValido_DebeRetornarNull()
    {
        // Arrange
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "no-es-guid")
        ], "TestAuth");

        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public void ObtenerUsuarioId_SiExisteUsuarioAutenticado_DebeRetornarGuid()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
        ], "TestAuth");

        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioId();

        // Assert
        resultado.Should().Be(usuarioId);
    }
    
    [Fact]
    public void ObtenerUsuarioIdOpcional_SiNoTieneClaimDeIdentificacion_DebeRetornarNull()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[] { new Claim("otroClaim", "valor") }, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().BeNull();
    }
}