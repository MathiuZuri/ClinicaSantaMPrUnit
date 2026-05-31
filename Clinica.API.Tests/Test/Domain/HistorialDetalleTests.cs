using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class HistorialDetalleTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var detalle = new HistorialDetalle();

        // Assert
        detalle.Id.Should().NotBeEmpty();
        detalle.CodigoDetalle.Should().BeEmpty();

        detalle.HistorialClinicoId.Should().BeEmpty();
        detalle.HistorialClinico.Should().BeNull();

        detalle.TipoMovimiento.Should().Be(default);

        detalle.CitaId.Should().BeNull();
        detalle.Cita.Should().BeNull();

        detalle.AtencionId.Should().BeNull();
        detalle.Atencion.Should().BeNull();

        detalle.PagoId.Should().BeNull();
        detalle.Pago.Should().BeNull();

        detalle.Titulo.Should().BeEmpty();
        detalle.Descripcion.Should().BeEmpty();

        detalle.FechaRegistro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        detalle.UsuarioId.Should().BeNull();
        detalle.Usuario.Should().BeNull();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var citaId = Guid.NewGuid();
        var atencionId = Guid.NewGuid();
        var pagoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var fechaRegistro = new DateTime(2026, 1, 10, 9, 5, 0, DateTimeKind.Utc);

        // Act
        var detalle = new HistorialDetalle
        {
            CodigoDetalle = "ABCDE-REG-2026-12345678",
            HistorialClinicoId = historialId,
            TipoMovimiento = TipoMovimientoHistorial.AperturaHistorial,
            CitaId = citaId,
            AtencionId = atencionId,
            PagoId = pagoId,
            Titulo = "Apertura de historial clínico",
            Descripcion = "Se aperturó el historial clínico del paciente.",
            FechaRegistro = fechaRegistro,
            UsuarioId = usuarioId
        };

        // Assert
        detalle.CodigoDetalle.Should().Be("ABCDE-REG-2026-12345678");
        detalle.HistorialClinicoId.Should().Be(historialId);
        detalle.TipoMovimiento.Should().Be(TipoMovimientoHistorial.AperturaHistorial);
        detalle.CitaId.Should().Be(citaId);
        detalle.AtencionId.Should().Be(atencionId);
        detalle.PagoId.Should().Be(pagoId);
        detalle.Titulo.Should().Be("Apertura de historial clínico");
        detalle.Descripcion.Should().Be("Se aperturó el historial clínico del paciente.");
        detalle.FechaRegistro.Should().Be(fechaRegistro);
        detalle.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void TipoMovimiento_DebePoderCambiar()
    {
        // Arrange
        var detalle = new HistorialDetalle();

        // Act
        detalle.TipoMovimiento = TipoMovimientoHistorial.CitaProgramada;
        detalle.TipoMovimiento = TipoMovimientoHistorial.CitaCancelada;

        // Assert
        detalle.TipoMovimiento.Should().Be(TipoMovimientoHistorial.CitaCancelada);
    }

    [Fact]
    public void RelacionesOpcionales_DebenPermitirValoresNulos()
    {
        // Arrange
        var detalle = new HistorialDetalle
        {
            TipoMovimiento = TipoMovimientoHistorial.ObservacionClinica,
            Titulo = "Observación clínica",
            Descripcion = "Se registró una observación clínica."
        };

        // Assert
        detalle.CitaId.Should().BeNull();
        detalle.AtencionId.Should().BeNull();
        detalle.PagoId.Should().BeNull();
        detalle.UsuarioId.Should().BeNull();
    }
}