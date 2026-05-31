using Clinica.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class PermisoTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var permiso = new Permiso();

        // Assert
        permiso.Id.Should().NotBeEmpty();
        permiso.Codigo.Should().BeEmpty();
        permiso.Nombre.Should().BeEmpty();
        permiso.Modulo.Should().BeEmpty();
        permiso.Descripcion.Should().BeNull();
        permiso.Activo.Should().BeTrue();
        permiso.RolPermisos.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Act
        var permiso = new Permiso
        {
            Codigo = "PACIENTE_VER",
            Nombre = "Ver pacientes",
            Modulo = "Pacientes",
            Descripcion = "Permite visualizar el listado de pacientes.",
            Activo = false
        };

        // Assert
        permiso.Codigo.Should().Be("PACIENTE_VER");
        permiso.Nombre.Should().Be("Ver pacientes");
        permiso.Modulo.Should().Be("Pacientes");
        permiso.Descripcion.Should().Be("Permite visualizar el listado de pacientes.");
        permiso.Activo.Should().BeFalse();
    }

    [Fact]
    public void ColeccionRolPermisos_DebePermitirAgregarElementos()
    {
        // Arrange
        var permiso = new Permiso();

        // Act
        permiso.RolPermisos.Add(new RolPermiso());

        // Assert
        permiso.RolPermisos.Should().HaveCount(1);
    }

    [Fact]
    public void Activo_DebePoderCambiar()
    {
        // Arrange
        var permiso = new Permiso();

        // Act
        permiso.Activo = false;

        // Assert
        permiso.Activo.Should().BeFalse();
    }
}