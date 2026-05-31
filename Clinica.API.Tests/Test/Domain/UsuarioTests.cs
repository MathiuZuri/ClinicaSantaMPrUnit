using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class UsuarioTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var usuario = new Usuario();

        // Assert
        usuario.Id.Should().NotBeEmpty();
        usuario.CodigoUsuario.Should().BeEmpty();
        usuario.Nombres.Should().BeEmpty();
        usuario.Apellidos.Should().BeEmpty();
        usuario.UserName.Should().BeEmpty();
        usuario.Correo.Should().BeEmpty();
        usuario.PasswordHash.Should().BeEmpty();
        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        usuario.FechaRegistro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        usuario.UltimoAcceso.Should().BeNull();

        usuario.UsuarioRoles.Should().NotBeNull().And.BeEmpty();
        usuario.Auditorias.Should().NotBeNull().And.BeEmpty();
        usuario.ComprobantesEmitidos.Should().NotBeNull().And.BeEmpty();
        usuario.ComprobantesAnulados.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var fechaRegistro = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc);
        var ultimoAcceso = new DateTime(2026, 1, 11, 9, 0, 0, DateTimeKind.Utc);

        // Act
        var usuario = new Usuario
        {
            CodigoUsuario = "USR-2026-ABCDE",
            Nombres = "Kevin",
            Apellidos = "Paricahua",
            UserName = "kevin.paricahua",
            Correo = "kevin@correo.com",
            PasswordHash = "hash-seguro",
            Estado = EstadoUsuario.Bloqueado,
            FechaRegistro = fechaRegistro,
            UltimoAcceso = ultimoAcceso
        };

        // Assert
        usuario.CodigoUsuario.Should().Be("USR-2026-ABCDE");
        usuario.Nombres.Should().Be("Kevin");
        usuario.Apellidos.Should().Be("Paricahua");
        usuario.UserName.Should().Be("kevin.paricahua");
        usuario.Correo.Should().Be("kevin@correo.com");
        usuario.PasswordHash.Should().Be("hash-seguro");
        usuario.Estado.Should().Be(EstadoUsuario.Bloqueado);
        usuario.FechaRegistro.Should().Be(fechaRegistro);
        usuario.UltimoAcceso.Should().Be(ultimoAcceso);
    }

    [Fact]
    public void Colecciones_DebenPermitirAgregarElementos()
    {
        // Arrange
        var usuario = new Usuario();

        // Act
        usuario.UsuarioRoles.Add(new UsuarioRol());
        usuario.Auditorias.Add(new Auditoria());
        usuario.ComprobantesEmitidos.Add(new Comprobante());
        usuario.ComprobantesAnulados.Add(new Comprobante());

        // Assert
        usuario.UsuarioRoles.Should().HaveCount(1);
        usuario.Auditorias.Should().HaveCount(1);
        usuario.ComprobantesEmitidos.Should().HaveCount(1);
        usuario.ComprobantesAnulados.Should().HaveCount(1);
    }

    [Fact]
    public void Estado_DebePoderCambiar()
    {
        // Arrange
        var usuario = new Usuario();

        // Act
        usuario.Estado = EstadoUsuario.Inactivo;
        usuario.Estado = EstadoUsuario.Eliminado;

        // Assert
        usuario.Estado.Should().Be(EstadoUsuario.Eliminado);
    }
}