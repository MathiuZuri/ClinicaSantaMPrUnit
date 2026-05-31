using Clinica.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class RolPermisoTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var rolPermiso = new RolPermiso();

        // Assert
        rolPermiso.Id.Should().NotBeEmpty();

        rolPermiso.RolId.Should().BeEmpty();
        rolPermiso.Rol.Should().BeNull();

        rolPermiso.PermisoId.Should().BeEmpty();
        rolPermiso.Permiso.Should().BeNull();

        rolPermiso.FechaAsignacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var rolId = Guid.NewGuid();
        var permisoId = Guid.NewGuid();
        var fechaAsignacion = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc);

        // Act
        var rolPermiso = new RolPermiso
        {
            RolId = rolId,
            PermisoId = permisoId,
            FechaAsignacion = fechaAsignacion
        };

        // Assert
        rolPermiso.RolId.Should().Be(rolId);
        rolPermiso.PermisoId.Should().Be(permisoId);
        rolPermiso.FechaAsignacion.Should().Be(fechaAsignacion);
    }

    [Fact]
    public void DebePermitirAsignarRelaciones()
    {
        // Arrange
        var rol = new Rol
        {
            Id = Guid.NewGuid(),
            Nombre = "Administrador"
        };

        var permiso = new Permiso
        {
            Id = Guid.NewGuid(),
            Codigo = "USUARIO_VER",
            Nombre = "Ver usuarios",
            Modulo = "Usuarios"
        };

        // Act
        var rolPermiso = new RolPermiso
        {
            RolId = rol.Id,
            Rol = rol,
            PermisoId = permiso.Id,
            Permiso = permiso
        };

        // Assert
        rolPermiso.Rol.Should().NotBeNull();
        rolPermiso.Rol.Id.Should().Be(rol.Id);
        rolPermiso.Rol.Nombre.Should().Be("Administrador");

        rolPermiso.Permiso.Should().NotBeNull();
        rolPermiso.Permiso.Id.Should().Be(permiso.Id);
        rolPermiso.Permiso.Codigo.Should().Be("USUARIO_VER");
    }
}