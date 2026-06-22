using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class ComprobanteTests
{
    [Fact]
    public void Constructor_ValoresPorDefecto()
    {
        var c = new Comprobante();
        c.Id.Should().NotBeEmpty();
        c.Estado.Should().Be(EstadoComprobante.Emitido);
        c.FormatoImpresion.Should().Be(TipoFormatoImpresion.A4);
        c.Detalles.Should().BeEmpty();
    }

    [Fact]
    public void AsignarPropiedades_ReflejaValores()
    {
        var id = Guid.NewGuid();
        var c = new Comprobante
        {
            Id = id,
            CodigoComprobante = "B001-000001",
            TipoComprobante = TipoComprobante.BoletaPago,
            Estado = EstadoComprobante.Anulado,
            Total = 100
        };
        c.CodigoComprobante.Should().Be("B001-000001");
        c.Estado.Should().Be(EstadoComprobante.Anulado);
    }
}