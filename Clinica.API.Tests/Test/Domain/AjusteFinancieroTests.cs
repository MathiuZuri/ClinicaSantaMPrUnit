using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class AjusteFinancieroTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var ajuste = new AjusteFinanciero();

        // Assert
        ajuste.Id.Should().NotBeEmpty();

        ajuste.PagoId.Should().BeEmpty();
        ajuste.Pago.Should().BeNull();

        ajuste.AtencionId.Should().BeNull();
        ajuste.Atencion.Should().BeNull();

        ajuste.PacienteId.Should().BeEmpty();
        ajuste.Paciente.Should().BeNull();

        ajuste.TipoAjuste.Should().Be(default);
        ajuste.MontoAjuste.Should().Be(0);
        ajuste.Motivo.Should().BeEmpty();
        ajuste.Observacion.Should().BeNull();

        ajuste.UsuarioRegistroId.Should().BeNull();
        ajuste.UsuarioRegistro.Should().BeNull();

        ajuste.FechaRegistro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var pagoId = Guid.NewGuid();
        var atencionId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var fecha = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var ajuste = new AjusteFinanciero
        {
            PagoId = pagoId,
            AtencionId = atencionId,
            PacienteId = pacienteId,
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 25m,
            Motivo = "Descuento autorizado",
            Observacion = "Paciente recurrente",
            UsuarioRegistroId = usuarioId,
            FechaRegistro = fecha
        };

        // Assert
        ajuste.PagoId.Should().Be(pagoId);
        ajuste.AtencionId.Should().Be(atencionId);
        ajuste.PacienteId.Should().Be(pacienteId);
        ajuste.TipoAjuste.Should().Be(TipoAjusteFinanciero.Descuento);
        ajuste.MontoAjuste.Should().Be(25m);
        ajuste.Motivo.Should().Be("Descuento autorizado");
        ajuste.Observacion.Should().Be("Paciente recurrente");
        ajuste.UsuarioRegistroId.Should().Be(usuarioId);
        ajuste.FechaRegistro.Should().Be(fecha);
    }

    [Fact]
    public void TipoAjuste_DebePoderCambiar()
    {
        // Arrange
        var ajuste = new AjusteFinanciero();

        // Act
        ajuste.TipoAjuste = TipoAjusteFinanciero.Recargo;
        ajuste.TipoAjuste = TipoAjusteFinanciero.Reembolso;

        // Assert
        ajuste.TipoAjuste.Should().Be(TipoAjusteFinanciero.Reembolso);
    }
}