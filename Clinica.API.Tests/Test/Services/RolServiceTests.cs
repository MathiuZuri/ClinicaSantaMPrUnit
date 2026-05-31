using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Roles;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class RolServiceTests
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IRolService _service;

    public RolServiceTests()
    {
        _rolRepository = Substitute.For<IRolRepository>();
        _permisoRepository = Substitute.For<IPermisoRepository>();
        _service = new RolService(_rolRepository, _permisoRepository);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var roles = new List<Rol>
        {
            CrearRolEntidad(),
            CrearRolEntidad()
        };

        _rolRepository.GetAllAsync().Returns(roles);

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(roles[0].Id);
        resultado[0].Nombre.Should().Be(roles[0].Nombre);
        resultado[0].Descripcion.Should().Be(roles[0].Descripcion);
        resultado[0].EsSistema.Should().Be(roles[0].EsSistema);
        resultado[0].Activo.Should().Be(roles[0].Activo);
        resultado[0].FechaCreacion.Should().Be(roles[0].FechaCreacion);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var rol = CrearRolEntidad();
        _rolRepository.GetByIdAsync(rol.Id).Returns(rol);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(rol.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(rol.Id);
        resultado.Nombre.Should().Be(rol.Nombre);
        resultado.Descripcion.Should().Be(rol.Descripcion);
        resultado.EsSistema.Should().Be(rol.EsSistema);
        resultado.Activo.Should().Be(rol.Activo);
        resultado.FechaCreacion.Should().Be(rol.FechaCreacion);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _rolRepository.GetByIdAsync(id).Returns((Rol?)null);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearAsync_SiNombreYaExiste_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new CrearRolDto
        {
            Nombre = "Administrador",
            Descripcion = "Rol principal"
        };

        _rolRepository.ObtenerPorNombreAsync(dto.Nombre).Returns(CrearRolEntidad(nombre: dto.Nombre));

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe un rol con ese nombre.");

        await _rolRepository.DidNotReceive().AddAsync(Arg.Any<Rol>());
    }

    [Fact]
    public async Task CrearAsync_SiNombreNoExiste_DebeCrearRolYGuardar()
    {
        // Arrange
        var dto = new CrearRolDto
        {
            Nombre = "Recepcionista",
            Descripcion = "Rol de recepción"
        };

        _rolRepository.ObtenerPorNombreAsync(dto.Nombre).Returns((Rol?)null);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _rolRepository.Received(1).AddAsync(Arg.Is<Rol>(r =>
            r.Nombre == dto.Nombre &&
            r.Descripcion == dto.Descripcion &&
            r.EsSistema == false &&
            r.Activo == true));

        await _rolRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ActualizarAsync_SiRolNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new EditarRolDto
        {
            Nombre = "Caja",
            Descripcion = "Rol actualizado",
            Activo = false
        };

        _rolRepository.GetByIdAsync(id).Returns((Rol?)null);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Rol no encontrado.");
    }

    [Fact]
    public async Task ActualizarAsync_SiRolEsSistema_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rol = CrearRolEntidad(id: id, esSistema: true);

        var dto = new EditarRolDto
        {
            Nombre = "Administrador Editado",
            Descripcion = "No debería editarse",
            Activo = false
        };

        _rolRepository.GetByIdAsync(id).Returns(rol);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede editar un rol del sistema.");

        _rolRepository.DidNotReceive().Update(Arg.Any<Rol>());
    }

    [Fact]
    public async Task ActualizarAsync_SiRolExisteYNoEsSistema_DebeActualizarYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rol = CrearRolEntidad(id: id, esSistema: false);

        var dto = new EditarRolDto
        {
            Nombre = "Recepcionista Senior",
            Descripcion = "Rol actualizado",
            Activo = false
        };

        _rolRepository.GetByIdAsync(id).Returns(rol);

        // Act
        await _service.ActualizarAsync(id, dto);

        // Assert
        rol.Nombre.Should().Be(dto.Nombre);
        rol.Descripcion.Should().Be(dto.Descripcion);
        rol.Activo.Should().Be(dto.Activo);

        _rolRepository.Received(1).Update(rol);
        await _rolRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task AsignarPermisosAsync_SiRolNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = new AsignarPermisosRolDto
        {
            RolId = Guid.NewGuid(),
            PermisosIds = new List<Guid> { Guid.NewGuid() }
        };

        _rolRepository.GetByIdAsync(dto.RolId).Returns((Rol?)null);

        // Act
        Func<Task> act = async () => await _service.AsignarPermisosAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Rol no encontrado.");
    }

    [Fact]
    public async Task AsignarPermisosAsync_SiPermisosExisten_DebeAgregarRolPermisosYGuardar()
    {
        // Arrange
        var permisoId1 = Guid.NewGuid();
        var permisoId2 = Guid.NewGuid();

        var dto = new AsignarPermisosRolDto
        {
            RolId = Guid.NewGuid(),
            PermisosIds = new List<Guid> { permisoId1, permisoId2 }
        };

        var rol = CrearRolEntidad(id: dto.RolId);

        _rolRepository.GetByIdAsync(dto.RolId).Returns(rol);

        _permisoRepository.GetByIdAsync(permisoId1)
            .Returns(new Permiso { Id = permisoId1 });

        _permisoRepository.GetByIdAsync(permisoId2)
            .Returns(new Permiso { Id = permisoId2 });

        _rolRepository.TienePermisoAsignadoAsync(dto.RolId, permisoId1)
            .Returns(false);

        _rolRepository.TienePermisoAsignadoAsync(dto.RolId, permisoId2)
            .Returns(false);

        // Act
        await _service.AsignarPermisosAsync(dto);

        // Assert
        await _rolRepository.Received(1).AgregarPermisoAsync(
            Arg.Is<RolPermiso>(x =>
                x.RolId == dto.RolId &&
                x.PermisoId == permisoId1)
        );

        await _rolRepository.Received(1).AgregarPermisoAsync(
            Arg.Is<RolPermiso>(x =>
                x.RolId == dto.RolId &&
                x.PermisoId == permisoId2)
        );

        _rolRepository.DidNotReceive().Update(Arg.Any<Rol>());

        await _rolRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task AsignarPermisosAsync_SiPermisoYaEstaAsignado_NoDebeDuplicarPeroDebeGuardar()
    {
        // Arrange
        var permisoId = Guid.NewGuid();

        var dto = new AsignarPermisosRolDto
        {
            RolId = Guid.NewGuid(),
            PermisosIds = new List<Guid> { permisoId }
        };

        var rol = CrearRolEntidad(id: dto.RolId);

        _rolRepository.GetByIdAsync(dto.RolId).Returns(rol);

        _permisoRepository.GetByIdAsync(permisoId)
            .Returns(new Permiso { Id = permisoId });

        _rolRepository.TienePermisoAsignadoAsync(dto.RolId, permisoId)
            .Returns(true);

        // Act
        await _service.AsignarPermisosAsync(dto);

        // Assert
        await _rolRepository.DidNotReceive().AgregarPermisoAsync(
            Arg.Any<RolPermiso>()
        );

        _rolRepository.DidNotReceive().Update(Arg.Any<Rol>());

        await _rolRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task AsignarPermisosAsync_SiAlgunPermisoNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var permisoValido = Guid.NewGuid();
        var permisoInvalido = Guid.NewGuid();

        var dto = new AsignarPermisosRolDto
        {
            RolId = Guid.NewGuid(),
            PermisosIds = new List<Guid> { permisoValido, permisoInvalido }
        };

        var rol = CrearRolEntidad(id: dto.RolId);

        _rolRepository.GetByIdAsync(dto.RolId).Returns(rol);

        _permisoRepository.GetByIdAsync(permisoValido)
            .Returns(new Permiso { Id = permisoValido });

        _permisoRepository.GetByIdAsync(permisoInvalido)
            .Returns((Permiso?)null);

        _rolRepository.TienePermisoAsignadoAsync(dto.RolId, permisoValido)
            .Returns(false);

        // Act
        var act = async () => await _service.AsignarPermisosAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Uno o más permisos no fueron encontrados.");

        await _rolRepository.Received(1).AgregarPermisoAsync(
            Arg.Is<RolPermiso>(x =>
                x.RolId == dto.RolId &&
                x.PermisoId == permisoValido)
        );

        await _rolRepository.DidNotReceive().AgregarPermisoAsync(
            Arg.Is<RolPermiso>(x =>
                x.PermisoId == permisoInvalido)
        );

        await _rolRepository.DidNotReceive().SaveChangesAsync();
    }

    private static Rol CrearRolEntidad(Guid? id = null, string nombre = "Administrador", bool esSistema = false)
    {
        return new Rol
        {
            Id = id ?? Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = "Rol de prueba",
            EsSistema = esSistema,
            Activo = true,
            FechaCreacion = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc)
        };
    }
}