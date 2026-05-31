using Clinica.Domain.DTOs.Servicios;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class ServicioClinicoResponseDtoTests
{
    [Fact]
    public void DebePermitirAsignarPropiedades()
    {
        var dto = new ServicioClinicoResponseDto
        {
            Id = Guid.NewGuid(),
            CodigoServicio = "TEST",
            Nombre = "Test Service",
            Descripcion = "Desc",
            CostoBase = 100,
            DuracionMinutos = 15,
            RequiereCita = false,
            GeneraHistorial = true,
            Estado = EstadoServicioClinico.Activo
        };

        dto.Id.Should().NotBeEmpty();
        dto.CodigoServicio.Should().Be("TEST");
        dto.Nombre.Should().Be("Test Service");
        dto.Descripcion.Should().Be("Desc");
        dto.CostoBase.Should().Be(100);
        dto.DuracionMinutos.Should().Be(15);
        dto.RequiereCita.Should().BeFalse();
        dto.GeneraHistorial.Should().BeTrue();
        dto.Estado.Should().Be(EstadoServicioClinico.Activo);
    }
}