using System.IdentityModel.Tokens.Jwt;
using Clinica.API.Helpers;
using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Auth;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class AuthServiceTests
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly IAuthService _service;

    public AuthServiceTests()
    {
        _usuarioRepository = Substitute.For<IUsuarioRepository>();

        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "EstaEsUnaClaveMuySeguraParaPruebas123456789",
            ["Jwt:Issuer"] = "Clinica.API.Tests",
            ["Jwt:Audience"] = "Clinica.API.Tests.Users",
            ["Jwt:ExpireMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _jwtHelper = new JwtHelper(configuration);
        _service = new AuthService(_usuarioRepository, _jwtHelper);
    }

    [Fact]
    public async Task IniciarSesionAsync_SiUsuarioNoExistePorCorreoNiUserName_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "noexiste@correo.com",
            Password = "Password123"
        };

        _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo).Returns((Usuario?)null);
        _usuarioRepository.ObtenerPorUserNameAsync(dto.UsuarioOCorreo).Returns((Usuario?)null);

        // Act
        Func<Task> act = async () => await _service.IniciarSesionAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Usuario o contraseña incorrectos.");
    }

    [Fact]
    public async Task IniciarSesionAsync_SiPasswordEsIncorrecta_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "PasswordIncorrecta"
        };

        var usuario = CrearUsuarioEntidad();
        _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo).Returns(usuario);

        // Act
        Func<Task> act = async () => await _service.IniciarSesionAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Usuario o contraseña incorrectos.");
    }

    [Fact]
    public async Task IniciarSesionAsync_SiUsuarioExistePorCorreo_DebeRetornarRespuestaConTokenRolesYPermisos()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "Password123!"
        };

        var usuario = CrearUsuarioEntidad();
        _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo).Returns(usuario);

        // Act
        var resultado = await _service.IniciarSesionAsync(dto);

        // Assert
        resultado.UsuarioId.Should().Be(usuario.Id);
        resultado.CodigoUsuario.Should().Be(usuario.CodigoUsuario);
        resultado.NombreCompleto.Should().Be($"{usuario.Nombres} {usuario.Apellidos}");
        resultado.Correo.Should().Be(usuario.Correo);
        resultado.Token.Should().NotBeNullOrWhiteSpace();

        resultado.Roles.Should().Contain("Administrador");
        resultado.Roles.Should().Contain("Doctor");
        resultado.Roles.Should().OnlyHaveUniqueItems();

        resultado.Permisos.Should().Contain("USUARIO_VER");
        resultado.Permisos.Should().Contain("PACIENTE_VER");
        resultado.Permisos.Should().Contain("CITA_PROGRAMAR");
        resultado.Permisos.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task IniciarSesionAsync_SiUsuarioNoExistePorCorreoPeroSiPorUserName_DebeAutenticarCorrectamente()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin.paricahua",
            Password = "Password123!"
        };

        var usuario = CrearUsuarioEntidad(userName: "kevin.paricahua");
        _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo).Returns((Usuario?)null);
        _usuarioRepository.ObtenerPorUserNameAsync(dto.UsuarioOCorreo).Returns(usuario);

        // Act
        var resultado = await _service.IniciarSesionAsync(dto);

        // Assert
        resultado.UsuarioId.Should().Be(usuario.Id);
        resultado.Token.Should().NotBeNullOrWhiteSpace();
        resultado.Roles.Should().NotBeEmpty();
        resultado.Permisos.Should().NotBeEmpty();
    }

    [Fact]
    public async Task IniciarSesionAsync_SoloDebeTomarRolesActivos()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "Password123!"
        };

        var usuario = CrearUsuarioEntidad();
        usuario.UsuarioRoles.Add(new UsuarioRol
        {
            UsuarioId = usuario.Id,
            RolId = Guid.NewGuid(),
            Activo = false,
            Rol = new Rol
            {
                Nombre = "RolInactivo",
                RolPermisos = new List<RolPermiso>
                {
                    new()
                    {
                        Permiso = new Permiso { Codigo = "NO_DEBE_APARECER" }
                    }
                }
            }
        });

        _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo).Returns(usuario);

        // Act
        var resultado = await _service.IniciarSesionAsync(dto);

        // Assert
        resultado.Roles.Should().NotContain("RolInactivo");
        resultado.Permisos.Should().NotContain("NO_DEBE_APARECER");
    }

    [Fact]
    public async Task IniciarSesionAsync_DebeGenerarTokenJwtValidoConClaimsEsperados()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "Password123!"
        };

        var usuario = CrearUsuarioEntidad();
        _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo).Returns(usuario);

        // Act
        var resultado = await _service.IniciarSesionAsync(dto);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);

        jwt.Claims.Should().Contain(c => c.Type.EndsWith("nameidentifier") && c.Value == usuario.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type.EndsWith("emailaddress") && c.Value == usuario.Correo);
        jwt.Claims.Should().Contain(c => c.Type == "codigoUsuario" && c.Value == usuario.CodigoUsuario);
        jwt.Claims.Should().Contain(c => c.Type.EndsWith("role") && c.Value == "Administrador");
        jwt.Claims.Should().Contain(c => c.Type == "permiso" && c.Value == "USUARIO_VER");
    }

    private static Usuario CrearUsuarioEntidad(string correo = "kevin@correo.com", string userName = "kevin.paricahua")
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            CodigoUsuario = "USR-2026-ABCDE",
            Nombres = "Kevin",
            Apellidos = "Paricahua",
            UserName = userName,
            Correo = correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };

        usuario.UsuarioRoles.Add(new UsuarioRol
        {
            UsuarioId = usuario.Id,
            RolId = Guid.NewGuid(),
            Activo = true,
            Rol = new Rol
            {
                Nombre = "Administrador",
                RolPermisos = new List<RolPermiso>
                {
                    new()
                    {
                        Permiso = new Permiso { Codigo = "USUARIO_VER" }
                    },
                    new()
                    {
                        Permiso = new Permiso { Codigo = "PACIENTE_VER" }
                    }
                }
            }
        });

        usuario.UsuarioRoles.Add(new UsuarioRol
        {
            UsuarioId = usuario.Id,
            RolId = Guid.NewGuid(),
            Activo = true,
            Rol = new Rol
            {
                Nombre = "Doctor",
                RolPermisos = new List<RolPermiso>
                {
                    new()
                    {
                        Permiso = new Permiso { Codigo = "CITA_PROGRAMAR" }
                    },
                    new()
                    {
                        Permiso = new Permiso { Codigo = "PACIENTE_VER" }
                    }
                }
            }
        });

        return usuario;
    }
}