using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class PagoTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var pago = new Pago();

        // Assert
        pago.Id.Should().NotBeEmpty();
        pago.CodigoPago.Should().BeEmpty();

        pago.PacienteId.Should().BeEmpty();
        pago.Paciente.Should().BeNull();

        pago.ServicioClinicoId.Should().BeEmpty();
        pago.ServicioClinico.Should().BeNull();

        pago.CitaId.Should().BeNull();
        pago.Cita.Should().BeNull();

        pago.AtencionId.Should().BeNull();
        pago.Atencion.Should().BeNull();

        pago.MontoTotal.Should().Be(0);
        pago.MontoPagado.Should().Be(0);
        pago.SaldoPendiente.Should().Be(0);
        pago.MontoAdelanto.Should().Be(0);

        pago.MetodoPago.Should().Be(default);
        pago.Estado.Should().Be(EstadoPago.Pendiente);

        pago.Observacion.Should().BeNull();
        pago.FechaPago.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        pago.UsuarioRegistroId.Should().BeNull();
        pago.UsuarioRegistro.Should().BeNull();

        pago.HistorialDetalles.Should().NotBeNull().And.BeEmpty();
        pago.AjustesFinancieros.Should().NotBeNull().And.BeEmpty();
        pago.Comprobantes.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var servicioId = Guid.NewGuid();
        var citaId = Guid.NewGuid();
        var atencionId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var fechaPago = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc);

        // Act
        var pago = new Pago
        {
            CodigoPago = "ABCDE-PAG-2026-12345678",
            PacienteId = pacienteId,
            ServicioClinicoId = servicioId,
            CitaId = citaId,
            AtencionId = atencionId,
            MontoTotal = 100m,
            MontoPagado = 60m,
            SaldoPendiente = 40m,
            MontoAdelanto = 20m,
            MetodoPago = MetodoPago.Yape,
            Estado = EstadoPago.Parcial,
            Observacion = "Pago parcial",
            FechaPago = fechaPago,
            UsuarioRegistroId = usuarioId
        };

        // Assert
        pago.CodigoPago.Should().Be("ABCDE-PAG-2026-12345678");
        pago.PacienteId.Should().Be(pacienteId);
        pago.ServicioClinicoId.Should().Be(servicioId);
        pago.CitaId.Should().Be(citaId);
        pago.AtencionId.Should().Be(atencionId);
        pago.MontoTotal.Should().Be(100m);
        pago.MontoPagado.Should().Be(60m);
        pago.SaldoPendiente.Should().Be(40m);
        pago.MontoAdelanto.Should().Be(20m);
        pago.MetodoPago.Should().Be(MetodoPago.Yape);
        pago.Estado.Should().Be(EstadoPago.Parcial);
        pago.Observacion.Should().Be("Pago parcial");
        pago.FechaPago.Should().Be(fechaPago);
        pago.UsuarioRegistroId.Should().Be(usuarioId);
    }

    [Fact]
    public void Colecciones_DebenPermitirAgregarElementos()
    {
        // Arrange
        var pago = new Pago();

        // Act
        pago.HistorialDetalles.Add(new HistorialDetalle());
        pago.AjustesFinancieros.Add(new AjusteFinanciero());
        pago.Comprobantes.Add(new Comprobante());

        // Assert
        pago.HistorialDetalles.Should().HaveCount(1);
        pago.AjustesFinancieros.Should().HaveCount(1);
        pago.Comprobantes.Should().HaveCount(1);
    }

    [Fact]
    public void Estado_DebePoderCambiar()
    {
        // Arrange
        var pago = new Pago();

        // Act
        pago.Estado = EstadoPago.Parcial;
        pago.Estado = EstadoPago.Pagado;

        // Assert
        pago.Estado.Should().Be(EstadoPago.Pagado);
    }
}