using Clinica.API.Authorization;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class PermisosPoliciesTests
{
    [Fact]
    public void Todos_DebeContenerPermisosImportantes()
    {
        // Assert
        PermisosPolicies.Todos.Should().Contain(PermisosPolicies.PacienteVer);
        PermisosPolicies.Todos.Should().Contain(PermisosPolicies.CitaProgramar);
        PermisosPolicies.Todos.Should().Contain(PermisosPolicies.DoctorEditar);
        PermisosPolicies.Todos.Should().Contain(PermisosPolicies.FinanzasVer);
        PermisosPolicies.Todos.Should().Contain(PermisosPolicies.UsuarioAsignarRol);
        PermisosPolicies.Todos.Should().Contain(PermisosPolicies.ComprobanteEmitir);
        PermisosPolicies.Todos.Should().Contain(PermisosPolicies.HistorialImprimir);
    }

    [Fact]
    public void Todos_NoDebeEstarVacio()
    {
        PermisosPolicies.Todos.Should().NotBeNull();
        PermisosPolicies.Todos.Should().NotBeEmpty();
    }

    [Fact]
    public void Constantes_DebenTenerElValorEsperado()
    {
        PermisosPolicies.PacienteVer.Should().Be("PACIENTE_VER");
        PermisosPolicies.PagoRegistrar.Should().Be("PAGO_REGISTRAR");
        PermisosPolicies.RolAsignarPermisos.Should().Be("ROL_ASIGNAR_PERMISOS");
        PermisosPolicies.AuditoriaVer.Should().Be("AUDITORIA_VER");
    }
}