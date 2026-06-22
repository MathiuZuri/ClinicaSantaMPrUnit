using Clinica.Domain.DTOs.Auditoria;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class AuditoriaResponseDtoTests
{
    [Fact]
    public void AsignarPropiedades_DebeFuncionar()
    {
        var dto = new AuditoriaResponseDto
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            UsuarioNombre = "Ana Prueba",
            TipoAccion = TipoAccionAuditoria.Creacion,
            Modulo = "Test",
            EntidadAfectada = "Entidad",
            EntidadId = Guid.NewGuid(),
            Descripcion = "Desc",
            ValorAnterior = "old",
            ValorNuevo = "new",
            IpAddress = "::1",
            UserAgent = "Mozilla",
            FueExitoso = true,
            DetalleError = null,
            Nivel = NivelAuditoria.Importante,
            FechaHora = DateTime.UtcNow,
            EsConsulta = false
        };

        dto.Id.Should().NotBeEmpty();
        dto.UsuarioNombre.Should().Be("Ana Prueba");
        dto.Nivel.Should().Be(NivelAuditoria.Importante);
        dto.EsConsulta.Should().BeFalse();
    }
}