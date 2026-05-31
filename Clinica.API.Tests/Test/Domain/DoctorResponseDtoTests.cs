using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class DoctorResponseDtoTests
{
    [Fact]
    public void NombreCompleto_DebeConcatenarNombresYApellidos()
    {
        // Arrange
        var dto = new DoctorResponseDto
        {
            Nombres = "Luis",
            Apellidos = "Mamani"
        };

        // Act
        var nombreCompleto = dto.NombreCompleto;

        // Assert
        nombreCompleto.Should().Be("Luis Mamani");
    }

    [Fact]
    public void DebePermitirAsignarPropiedades()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new DoctorResponseDto
        {
            Id = id,
            CodigoDoctor = "DOC-ABCDE-12345",
            CMP = "12345",
            Nombres = "Luis",
            Apellidos = "Mamani",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = "doctor@correo.com",
            FechaInicioContrato = new DateTime(2026, 1, 10),
            FechaFinContrato = new DateTime(2026, 12, 31),
            Estado = EstadoDoctor.Activo
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.CodigoDoctor.Should().Be("DOC-ABCDE-12345");
        dto.CMP.Should().Be("12345");
        dto.NombreCompleto.Should().Be("Luis Mamani");
        dto.Estado.Should().Be(EstadoDoctor.Activo);
    }
}