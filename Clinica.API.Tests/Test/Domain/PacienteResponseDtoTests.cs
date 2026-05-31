using Clinica.Domain.DTOs.Pacientes;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class PacienteResponseDtoTests
{
    [Fact]
    public void NombreCompleto_DebeConcatenarNombresYApellidos()
    {
        // Arrange
        var dto = new PacienteResponseDto
        {
            Nombres = "Ana",
            Apellidos = "Quispe"
        };

        // Act
        var nombreCompleto = dto.NombreCompleto;

        // Assert
        nombreCompleto.Should().Be("Ana Quispe");
    }

    [Fact]
    public void DebePermitirAsignarPropiedades()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new PacienteResponseDto
        {
            Id = id,
            CodigoPaciente = "PCT-2026-ABCDE-12345678",
            DNI = "12345678",
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(2000, 5, 10),
            Sexo = "F",
            Celular = "987654321",
            Correo = "ana@correo.com",
            Direccion = "Juliaca",
            Estado = EstadoPaciente.Activo,
            FechaRegistro = new DateTime(2026, 1, 10),
            CodigoHistorial = "ABCDE-2026-12345678"
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.CodigoPaciente.Should().Be("PCT-2026-ABCDE-12345678");
        dto.DNI.Should().Be("12345678");
        dto.NombreCompleto.Should().Be("Ana Quispe");
        dto.Estado.Should().Be(EstadoPaciente.Activo);
        dto.CodigoHistorial.Should().Be("ABCDE-2026-12345678");
    }
}