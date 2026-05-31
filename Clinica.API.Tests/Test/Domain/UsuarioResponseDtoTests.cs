using Clinica.Domain.DTOs.Usuarios;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class UsuarioResponseDtoTests
{
    [Fact]
    public void NombreCompleto_DebeConcatenarNombresYApellidos()
    {
        // Arrange
        var dto = new UsuarioResponseDto
        {
            Nombres = "Kevin",
            Apellidos = "Paricahua"
        };

        // Act
        var nombreCompleto = dto.NombreCompleto;

        // Assert
        nombreCompleto.Should().Be("Kevin Paricahua");
    }

    [Fact]
    public void DebePermitirAsignarPropiedades()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ultimoAcceso = new DateTime(2026, 1, 10, 9, 30, 0);

        // Act
        var dto = new UsuarioResponseDto
        {
            Id = id,
            CodigoUsuario = "USR-2026-ABCDE",
            Nombres = "Kevin",
            Apellidos = "Paricahua",
            UserName = "kevin.paricahua",
            Correo = "kevin@correo.com",
            Estado = EstadoUsuario.Activo,
            FechaRegistro = new DateTime(2026, 1, 10, 8, 0, 0),
            UltimoAcceso = ultimoAcceso
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.CodigoUsuario.Should().Be("USR-2026-ABCDE");
        dto.NombreCompleto.Should().Be("Kevin Paricahua");
        dto.UserName.Should().Be("kevin.paricahua");
        dto.Correo.Should().Be("kevin@correo.com");
        dto.Estado.Should().Be(EstadoUsuario.Activo);
        dto.UltimoAcceso.Should().Be(ultimoAcceso);
    }
}