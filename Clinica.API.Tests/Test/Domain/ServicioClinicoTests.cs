using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class ServicioClinicoTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var servicio = new ServicioClinico();

        // Assert
        servicio.Id.Should().NotBeEmpty();
        servicio.CodigoServicio.Should().BeEmpty();
        servicio.Nombre.Should().BeEmpty();
        servicio.Descripcion.Should().BeNull();
        servicio.CostoBase.Should().Be(0);
        servicio.DuracionMinutos.Should().Be(0);
        servicio.RequiereCita.Should().BeTrue();
        servicio.GeneraHistorial.Should().BeTrue();
        servicio.Estado.Should().Be(EstadoServicioClinico.Activo);

        servicio.Citas.Should().NotBeNull().And.BeEmpty();
        servicio.Atenciones.Should().NotBeNull().And.BeEmpty();
        servicio.Pagos.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Act
        var servicio = new ServicioClinico
        {
            CodigoServicio = "CONOBS",
            Nombre = "Consulta obstétrica",
            Descripcion = "Consulta general especializada",
            CostoBase = 80.50m,
            DuracionMinutos = 30,
            RequiereCita = false,
            GeneraHistorial = false,
            Estado = EstadoServicioClinico.Inactivo
        };

        // Assert
        servicio.CodigoServicio.Should().Be("CONOBS");
        servicio.Nombre.Should().Be("Consulta obstétrica");
        servicio.Descripcion.Should().Be("Consulta general especializada");
        servicio.CostoBase.Should().Be(80.50m);
        servicio.DuracionMinutos.Should().Be(30);
        servicio.RequiereCita.Should().BeFalse();
        servicio.GeneraHistorial.Should().BeFalse();
        servicio.Estado.Should().Be(EstadoServicioClinico.Inactivo);
    }

    [Fact]
    public void Colecciones_DebenPermitirAgregarElementos()
    {
        // Arrange
        var servicio = new ServicioClinico();

        // Act
        servicio.Citas.Add(new Cita());
        servicio.Atenciones.Add(new Atencion());
        servicio.Pagos.Add(new Pago());

        // Assert
        servicio.Citas.Should().HaveCount(1);
        servicio.Atenciones.Should().HaveCount(1);
        servicio.Pagos.Should().HaveCount(1);
    }

    [Fact]
    public void Estado_DebePoderCambiar()
    {
        // Arrange
        var servicio = new ServicioClinico();

        // Act
        servicio.Estado = EstadoServicioClinico.Inactivo;
        servicio.Estado = EstadoServicioClinico.Eliminado;

        // Assert
        servicio.Estado.Should().Be(EstadoServicioClinico.Eliminado);
    }
}