using Clinica.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class RolTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var rol = new Rol();

        // Assert
        rol.Id.Should().NotBeEmpty();
        rol.Nombre.Should().BeEmpty();
        rol.Descripcion.Should().BeNull();
        rol.EsSistema.Should().BeFalse();
        rol.Activo.Should().BeTrue();
        rol.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        rol.UsuarioRoles.Should().NotBeNull().And.BeEmpty();
        rol.RolPermisos.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var fechaCreacion = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc);

        // Act
        var rol = new Rol
        {
            Nombre = "Administrador",
            Descripcion = "Rol con acceso total",
            EsSistema = true,
            Activo = false,
            FechaCreacion = fechaCreacion
        };

        // Assert
        rol.Nombre.Should().Be("Administrador");
        rol.Descripcion.Should().Be("Rol con acceso total");
        rol.EsSistema.Should().BeTrue();
        rol.Activo.Should().BeFalse();
        rol.FechaCreacion.Should().Be(fechaCreacion);
    }

    [Fact]
    public void Colecciones_DebenPermitirAgregarElementos()
    {
        // Arrange
        var rol = new Rol();

        // Act
        rol.UsuarioRoles.Add(new UsuarioRol());
        rol.RolPermisos.Add(new RolPermiso());

        // Assert
        rol.UsuarioRoles.Should().HaveCount(1);
        rol.RolPermisos.Should().HaveCount(1);
    }

    [Fact]
    public void PropiedadesBooleanas_DebenPoderCambiar()
    {
        // Arrange
        var rol = new Rol();

        // Act
        rol.EsSistema = true;
        rol.Activo = false;

        // Assert
        rol.EsSistema.Should().BeTrue();
        rol.Activo.Should().BeFalse();
    }
}