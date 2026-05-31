using System.Security.Claims;
using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Usuarios;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUsuarioService _service;

    public UsuarioServiceTests()
    {
        _usuarioRepository = Substitute.For<IUsuarioRepository>();
        _rolRepository = Substitute.For<IRolRepository>();
        _service = new UsuarioService(_usuarioRepository, _rolRepository);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            CrearUsuarioEntidad(),
            CrearUsuarioEntidad()
        };

        _usuarioRepository.GetAllAsync().Returns(usuarios);

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(usuarios[0].Id);
        resultado[0].CodigoUsuario.Should().Be(usuarios[0].CodigoUsuario);
        resultado[0].Nombres.Should().Be(usuarios[0].Nombres);
        resultado[0].Apellidos.Should().Be(usuarios[0].Apellidos);
        resultado[0].UserName.Should().Be(usuarios[0].UserName);
        resultado[0].Correo.Should().Be(usuarios[0].Correo);
        resultado[0].Estado.Should().Be(usuarios[0].Estado);
        resultado[0].FechaRegistro.Should().Be(usuarios[0].FechaRegistro);
        resultado[0].UltimoAcceso.Should().Be(usuarios[0].UltimoAcceso);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var usuario = CrearUsuarioEntidad();
        _usuarioRepository.GetByIdAsync(usuario.Id).Returns(usuario);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(usuario.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(usuario.Id);
        resultado.CodigoUsuario.Should().Be(usuario.CodigoUsuario);
        resultado.Nombres.Should().Be(usuario.Nombres);
        resultado.Apellidos.Should().Be(usuario.Apellidos);
        resultado.UserName.Should().Be(usuario.UserName);
        resultado.Correo.Should().Be(usuario.Correo);
        resultado.Estado.Should().Be(usuario.Estado);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _usuarioRepository.GetByIdAsync(id).Returns((Usuario?)null);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearAsync_SiCorreoExiste_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var existente = CrearUsuarioEntidad(correo: dto.Correo);

        _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo).Returns(existente);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe un usuario con ese correo.");

        await _usuarioRepository.DidNotReceive().AddAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task CrearAsync_SiUserNameExiste_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var existente = CrearUsuarioEntidad(userName: dto.UserName);

        _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo).Returns((Usuario?)null);
        _usuarioRepository.ObtenerPorUserNameAsync(dto.UserName).Returns(existente);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe un usuario con ese nombre de usuario.");

        await _usuarioRepository.DidNotReceive().AddAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task CrearAsync_SiTodoEsValido_DebeCrearUsuarioConPasswordHasheado()
    {
        // Arrange
        var dto = CrearDtoValido();

        _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo).Returns((Usuario?)null);
        _usuarioRepository.ObtenerPorUserNameAsync(dto.UserName).Returns((Usuario?)null);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _usuarioRepository.Received(1).AddAsync(Arg.Is<Usuario>(u =>
            u.Nombres == dto.Nombres &&
            u.Apellidos == dto.Apellidos &&
            u.UserName == dto.UserName &&
            u.Correo == dto.Correo &&
            !string.IsNullOrWhiteSpace(u.CodigoUsuario) &&
            u.CodigoUsuario.StartsWith($"USR-{DateTime.UtcNow:yyyy}-") &&
            !string.IsNullOrWhiteSpace(u.PasswordHash) &&
            u.PasswordHash != dto.Password &&
            BCrypt.Net.BCrypt.Verify(dto.Password, u.PasswordHash, false, BCrypt.Net.HashType.SHA384)));

        await _usuarioRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ActualizarAsync_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = CrearEditarDtoValido();

        _usuarioRepository.GetByIdAsync(id).Returns((Usuario?)null);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Usuario no encontrado.");
    }

    [Fact]
    public async Task ActualizarAsync_SiExiste_DebeActualizarYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuario = CrearUsuarioEntidad(id: id);
        var dto = CrearEditarDtoValido();

        _usuarioRepository.GetByIdAsync(id).Returns(usuario);

        // Act
        await _service.ActualizarAsync(id, dto);

        // Assert
        usuario.Nombres.Should().Be(dto.Nombres);
        usuario.Apellidos.Should().Be(dto.Apellidos);
        usuario.UserName.Should().Be(dto.UserName);
        usuario.Correo.Should().Be(dto.Correo);

        _usuarioRepository.Received(1).Update(usuario);
        await _usuarioRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task AsignarRolAsync_SiUsuarioNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = Guid.NewGuid(),
            RolId = Guid.NewGuid()
        };

        _usuarioRepository.GetByIdAsync(dto.UsuarioId).Returns((Usuario?)null);

        // Act
        Func<Task> act = async () => await _service.AsignarRolAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Usuario no encontrado.");
    }

    [Fact]
    public async Task AsignarRolAsync_SiRolNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = Guid.NewGuid(),
            RolId = Guid.NewGuid()
        };

        var usuario = CrearUsuarioEntidad(id: dto.UsuarioId);

        _usuarioRepository.GetByIdAsync(dto.UsuarioId).Returns(usuario);
        _rolRepository.GetByIdAsync(dto.RolId).Returns((Rol?)null);

        // Act
        Func<Task> act = async () => await _service.AsignarRolAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Rol no encontrado.");
    }

    [Fact]
    public async Task AsignarRolAsync_SiTodoEsValido_DebeAgregarUsuarioRolYGuardar()
    {
        // Arrange
        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = Guid.NewGuid(),
            RolId = Guid.NewGuid()
        };

        var usuario = CrearUsuarioEntidad(id: dto.UsuarioId);
        var rol = new Rol { Id = dto.RolId, Nombre = "Administrador" };

        _usuarioRepository.GetByIdAsync(dto.UsuarioId).Returns(usuario);
        _rolRepository.GetByIdAsync(dto.RolId).Returns(rol);

        _usuarioRepository.TieneRolAsignadoAsync(dto.UsuarioId, dto.RolId)
            .Returns(false);

        // Act
        await _service.AsignarRolAsync(dto);

        // Assert
        await _usuarioRepository.Received(1).AgregarRolAsync(
            Arg.Is<UsuarioRol>(x =>
                x.UsuarioId == dto.UsuarioId &&
                x.RolId == dto.RolId &&
                x.Activo)
        );

        _usuarioRepository.DidNotReceive().Update(Arg.Any<Usuario>());

        await _usuarioRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task AsignarRolAsync_SiUsuarioYaTieneRol_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new AsignarRolUsuarioDto
        {
            UsuarioId = Guid.NewGuid(),
            RolId = Guid.NewGuid()
        };

        var usuario = CrearUsuarioEntidad(id: dto.UsuarioId);
        var rol = new Rol { Id = dto.RolId, Nombre = "Administrador" };

        _usuarioRepository.GetByIdAsync(dto.UsuarioId).Returns(usuario);
        _rolRepository.GetByIdAsync(dto.RolId).Returns(rol);

        _usuarioRepository.TieneRolAsignadoAsync(dto.UsuarioId, dto.RolId)
            .Returns(true);

        // Act
        var act = async () => await _service.AsignarRolAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El usuario ya tiene asignado ese rol.");

        await _usuarioRepository.DidNotReceive().AgregarRolAsync(
            Arg.Any<UsuarioRol>()
        );

        await _usuarioRepository.DidNotReceive().SaveChangesAsync();
    }

    private static CrearUsuarioDto CrearDtoValido()
    {
        return new CrearUsuarioDto
        {
            Nombres = "Kevin",
            Apellidos = "Paricahua",
            UserName = "kevin.paricahua",
            Correo = "kevin@correo.com",
            Password = "Password123!"
        };
    }

    private static EditarUsuarioDto CrearEditarDtoValido()
    {
        return new EditarUsuarioDto
        {
            Nombres = "Carlos",
            Apellidos = "Mamani",
            UserName = "carlos.mamani",
            Correo = "carlos@correo.com"
        };
    }

    private static Usuario CrearUsuarioEntidad(
        Guid? id = null,
        string correo = "kevin@correo.com",
        string userName = "kevin.paricahua")
    {
        return new Usuario
        {
            Id = id ?? Guid.NewGuid(),
            CodigoUsuario = "USR-2026-ABCDE",
            Nombres = "Kevin",
            Apellidos = "Paricahua",
            UserName = userName,
            Correo = correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Estado = EstadoUsuario.Activo,
            FechaRegistro = DateTime.UtcNow
        };
    }
    [Fact]
    public async Task CambiarEstadoAsync_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var dto = new CambiarEstadoUsuarioDto { Estado = EstadoUsuario.Inactivo };

        _usuarioRepository.GetByIdAsync(id).Returns((Usuario?)null);

        Func<Task> act = async () => await _service.CambiarEstadoAsync(id, dto);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Usuario no encontrado.");
    }

    [Fact]
    public async Task CambiarEstadoAsync_SiEsAdminYEstadoNoEsActivo_DebeLanzarInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var usuario = CrearUsuarioEntidad(id: id);
        usuario.CodigoUsuario = "USR-ADMIN-2026-XXXXX"; // cumple condición admin
        var dto = new CambiarEstadoUsuarioDto { Estado = EstadoUsuario.Inactivo };

        _usuarioRepository.GetByIdAsync(id).Returns(usuario);

        Func<Task> act = async () => await _service.CambiarEstadoAsync(id, dto);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede desactivar al administrador principal del sistema.");
    }

    [Fact]
    public async Task CambiarEstadoAsync_SiEsAdminYEstadoEsActivo_DebeActualizar()
    {
        var id = Guid.NewGuid();
        var usuario = CrearUsuarioEntidad(id: id);
        usuario.CodigoUsuario = "USR-ADMIN-2026-YYYYY";
        var dto = new CambiarEstadoUsuarioDto { Estado = EstadoUsuario.Activo };

        _usuarioRepository.GetByIdAsync(id).Returns(usuario);

        await _service.CambiarEstadoAsync(id, dto);

        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        _usuarioRepository.Received(1).Update(usuario);
        await _usuarioRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CambiarEstadoAsync_SiNoEsAdmin_DebeActualizarEstado()
    {
        var id = Guid.NewGuid();
        var usuario = CrearUsuarioEntidad(id: id);
        usuario.CodigoUsuario = "USR-2026-ABCDE"; // no empieza con "USR-ADMIN"
        var dto = new CambiarEstadoUsuarioDto { Estado = EstadoUsuario.Bloqueado };

        _usuarioRepository.GetByIdAsync(id).Returns(usuario);

        await _service.CambiarEstadoAsync(id, dto);

        usuario.Estado.Should().Be(EstadoUsuario.Bloqueado);
        _usuarioRepository.Received(1).Update(usuario);
        await _usuarioRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public void ObtenerUsuarioIdOpcional_CuandoUserExistePeroIdentityEsNull_DebeRetornarNull()
    {
        // Arrange
        var context = new DefaultHttpContext { User = new ClaimsPrincipal() };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new UsuarioActualService(httpContextAccessor);

        // Act
        var resultado = service.ObtenerUsuarioIdOpcional();

        // Assert
        resultado.Should().BeNull();
    }
}