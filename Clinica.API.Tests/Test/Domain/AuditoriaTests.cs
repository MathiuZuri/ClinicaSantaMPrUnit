using Clinica.API.Filters;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class AuditoriaTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        var auditoria = new Auditoria();

        auditoria.Id.Should().NotBeEmpty();
        auditoria.UsuarioId.Should().BeNull();
        auditoria.Usuario.Should().BeNull();
        auditoria.TipoAccion.Should().Be(default);
        auditoria.Modulo.Should().BeEmpty();
        auditoria.EntidadAfectada.Should().BeEmpty();
        auditoria.EntidadId.Should().BeNull();
        auditoria.Descripcion.Should().BeEmpty();
        auditoria.ValorAnterior.Should().BeNull();
        auditoria.ValorNuevo.Should().BeNull();
        auditoria.IpAddress.Should().BeNull();
        auditoria.UserAgent.Should().BeNull();
        auditoria.FueExitoso.Should().BeTrue();
        auditoria.DetalleError.Should().BeNull();
        auditoria.Nivel.Should().Be(NivelAuditoria.Normal);
        auditoria.FechaHora.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        auditoria.EsConsulta.Should().BeFalse();
    }

    [Fact]
    public void AsignarPropiedades_DebeReflejarValores()
    {
        var id = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var ahora = DateTime.UtcNow;

        var auditoria = new Auditoria
        {
            Id = id,
            UsuarioId = usuarioId,
            TipoAccion = TipoAccionAuditoria.Eliminacion,
            Nivel = NivelAuditoria.Critico,
            Modulo = "Seguridad",
            EntidadAfectada = "Usuario",
            EntidadId = usuarioId,
            Descripcion = "Eliminación de usuario",
            ValorAnterior = "activo",
            ValorNuevo = "eliminado",
            IpAddress = "127.0.0.1",
            UserAgent = "test-agent",
            FueExitoso = false,
            DetalleError = "Error simulado",
            FechaHora = ahora,
            EsConsulta = true
        };

        auditoria.Id.Should().Be(id);
        auditoria.UsuarioId.Should().Be(usuarioId);
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Eliminacion);
        auditoria.Nivel.Should().Be(NivelAuditoria.Critico);
        auditoria.Modulo.Should().Be("Seguridad");
        auditoria.EntidadAfectada.Should().Be("Usuario");
        auditoria.EntidadId.Should().Be(usuarioId);
        auditoria.Descripcion.Should().Be("Eliminación de usuario");
        auditoria.ValorAnterior.Should().Be("activo");
        auditoria.ValorNuevo.Should().Be("eliminado");
        auditoria.IpAddress.Should().Be("127.0.0.1");
        auditoria.UserAgent.Should().Be("test-agent");
        auditoria.FueExitoso.Should().BeFalse();
        auditoria.DetalleError.Should().Be("Error simulado");
        auditoria.FechaHora.Should().Be(ahora);
        auditoria.EsConsulta.Should().BeTrue();
    }
    
    [Fact]
    public void AuditoriaAttribute_Constructor_AsignaPropiedades()
    {
        var attr = new AuditoriaAttribute("Modulo", "Entidad", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico);

        attr.Modulo.Should().Be("Modulo");
        attr.Entidad.Should().Be("Entidad");
        attr.TipoAccion.Should().Be(TipoAccionAuditoria.Creacion);
        attr.Nivel.Should().Be(NivelAuditoria.Critico);
    }
}