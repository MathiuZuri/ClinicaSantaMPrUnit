using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Clinica.API.Authorization;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Auth;
using FluentAssertions;
using System.Security.Claims;

namespace Clinica.API.IntegrationTests.PruebasInt.Auth;

[Collection("IntegrationTests")]
public class AuthEndpointsTests : IntegrationTestBase
{
    public AuthEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Post_Login_ConUsernameAdminValido_DeberiaRetornarOkYToken()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "admin",
            Password = "admin123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var login = await response.ReadDataAsJsonAsync<RespuestaInicioSesionDto>();

        login.Should().NotBeNull();
        login!.UsuarioId.Should().NotBeEmpty();
        login.CodigoUsuario.Should().NotBeNullOrWhiteSpace();
        login.NombreCompleto.Should().Be("Administrador Sistema");
        login.Correo.Should().Be("admin@clinica.com");
        login.Token.Should().NotBeNullOrWhiteSpace();
        login.Roles.Should().Contain("Administrador");
        login.Permisos.Should().NotBeEmpty();
        login.Permisos.Should().Contain(PermisosPolicies.PacienteVer);
        login.Permisos.Should().Contain(PermisosPolicies.CitaVer);
        login.Permisos.Should().Contain(PermisosPolicies.UsuarioVer);
        login.Permisos.Should().Contain(PermisosPolicies.RolVer);
    }

    [Fact]
    public async Task Post_Login_ConCorreoAdminValido_DeberiaRetornarOkYToken()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "admin@clinica.com",
            Password = "admin123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var login = await response.ReadDataAsJsonAsync<RespuestaInicioSesionDto>();

        login.Should().NotBeNull();
        login!.Correo.Should().Be("admin@clinica.com");
        login.Token.Should().NotBeNullOrWhiteSpace();
        login.Roles.Should().Contain("Administrador");
    }

    [Fact]
    public async Task Post_Login_ConCredencialesValidas_DeberiaRetornarJwtValido()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "admin",
            Password = "admin123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await response.ReadDataAsJsonAsync<RespuestaInicioSesionDto>();

        login.Should().NotBeNull();
        login!.Token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();

        handler.CanReadToken(login.Token).Should().BeTrue();

        var jwt = handler.ReadJwtToken(login.Token);

        jwt.Claims.Should().Contain(x =>
            x.Type == ClaimTypes.Email &&
            x.Value == "admin@clinica.com");

        jwt.Claims.Should().Contain(x =>
            x.Type == ClaimTypes.Name &&
            x.Value == "admin");

        jwt.Claims.Should().Contain(x =>
            x.Type == ClaimTypes.Role &&
            x.Value == "Administrador");

        jwt.Claims.Should().Contain(x =>
            x.Type == "codigoUsuario" &&
            x.Value == login.CodigoUsuario);

        jwt.Claims.Should().Contain(x =>
            x.Type == "permiso" &&
            x.Value == PermisosPolicies.PacienteVer);
    }

    [Fact]
    public async Task Post_Login_ConPasswordIncorrecto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "admin",
            Password = "passwordIncorrecto"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Login_ConUsuarioInexistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "usuario_inexistente",
            Password = "admin123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Login_ConUsuarioVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "",
            Password = "admin123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Login_ConUsuarioMuyCorto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "ad",
            Password = "admin123"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Login_ConPasswordVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "admin",
            Password = ""
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Login_ConPasswordMuyCorto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "admin",
            Password = "12345"
        };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Login_ConJsonVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var dto = new { };

        // Act
        var response = await Client.PostJsonAsync("/api/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Login_ConAdmin_DeberiaPermitirUsarTokenEnEndpointProtegido()
    {
        // Arrange
        var loginDto = new IniciarSesionDto
        {
            UsuarioOCorreo = "admin",
            Password = "admin123"
        };

        var loginResponse = await Client.PostJsonAsync("/api/auth/login", loginDto);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await loginResponse.ReadDataAsJsonAsync<RespuestaInicioSesionDto>();

        login.Should().NotBeNull();
        login!.Token.Should().NotBeNullOrWhiteSpace();

        Client.SetBearerToken(login.Token);

        // Act
        var response = await Client.GetAsync("/api/permisos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }
}