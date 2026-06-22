using Clinica.Domain.Entities.ATENCIONES;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class AtencionTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarColecciones()
    {
        var atencion = new Atencion();
        atencion.ExamenesFisicos.Should().NotBeNull().And.BeEmpty();
        atencion.TactosVaginales.Should().NotBeNull().And.BeEmpty();
        atencion.Ecografias.Should().NotBeNull().And.BeEmpty();
        atencion.Pagos.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AsignarEstado_DebeReflejarCambio()
    {
        var atencion = new Atencion();
        atencion.Estado = EstadoAtencion.Cerrada;
        atencion.Estado.Should().Be(EstadoAtencion.Cerrada);
    }
}