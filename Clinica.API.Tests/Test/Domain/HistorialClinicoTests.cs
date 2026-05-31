using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class HistorialClinicoTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var historial = new HistorialClinico();

        // Assert
        historial.Id.Should().NotBeEmpty();
        historial.CodigoHistorial.Should().BeEmpty();
        historial.PacienteId.Should().BeEmpty();
        historial.Paciente.Should().BeNull();
        historial.FechaApertura.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        historial.Estado.Should().Be(EstadoHistorialClinico.Activo);

        historial.Detalles.Should().NotBeNull().And.BeEmpty();
        historial.Comprobantes.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var fechaApertura = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc);

        // Act
        var historial = new HistorialClinico
        {
            CodigoHistorial = "ABCDE-2026-12345678",
            PacienteId = pacienteId,
            FechaApertura = fechaApertura,
            Estado = EstadoHistorialClinico.Cerrado
        };

        // Assert
        historial.CodigoHistorial.Should().Be("ABCDE-2026-12345678");
        historial.PacienteId.Should().Be(pacienteId);
        historial.FechaApertura.Should().Be(fechaApertura);
        historial.Estado.Should().Be(EstadoHistorialClinico.Cerrado);
    }

    [Fact]
    public void Colecciones_DebenPermitirAgregarElementos()
    {
        // Arrange
        var historial = new HistorialClinico();

        // Act
        historial.Detalles.Add(new HistorialDetalle());
        historial.Comprobantes.Add(new Comprobante());

        // Assert
        historial.Detalles.Should().HaveCount(1);
        historial.Comprobantes.Should().HaveCount(1);
    }

    [Fact]
    public void Estado_DebePoderCambiar()
    {
        // Arrange
        var historial = new HistorialClinico();

        // Act
        historial.Estado = EstadoHistorialClinico.Archivado;
        historial.Estado = EstadoHistorialClinico.Eliminado;

        // Assert
        historial.Estado.Should().Be(EstadoHistorialClinico.Eliminado);
    }
}