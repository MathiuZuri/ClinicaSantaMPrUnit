using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class DoctorTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var doctor = new Doctor();

        // Assert
        doctor.Id.Should().NotBeEmpty();
        doctor.CodigoDoctor.Should().BeEmpty();
        doctor.CMP.Should().BeEmpty();
        doctor.Nombres.Should().BeEmpty();
        doctor.Apellidos.Should().BeEmpty();
        doctor.Especialidad.Should().BeEmpty();
        doctor.Celular.Should().BeNull();
        doctor.Correo.Should().BeNull();
        doctor.FechaInicioContrato.Should().Be(default);
        doctor.FechaFinContrato.Should().BeNull();
        doctor.Estado.Should().Be(EstadoDoctor.Activo);
        doctor.UsuarioId.Should().BeEmpty();
        doctor.Usuario.Should().BeNull();

        doctor.Horarios.Should().NotBeNull().And.BeEmpty();
        doctor.Citas.Should().NotBeNull().And.BeEmpty();
        doctor.Atenciones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var fechaInicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var fechaFin = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var doctor = new Doctor
        {
            CodigoDoctor = "DOC-ABCDE-12345",
            CMP = "12345",
            Nombres = "Luis",
            Apellidos = "Mamani",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = "doctor@correo.com",
            FechaInicioContrato = fechaInicio,
            FechaFinContrato = fechaFin,
            Estado = EstadoDoctor.Inactivo,
            UsuarioId = usuarioId
        };

        // Assert
        doctor.CodigoDoctor.Should().Be("DOC-ABCDE-12345");
        doctor.CMP.Should().Be("12345");
        doctor.Nombres.Should().Be("Luis");
        doctor.Apellidos.Should().Be("Mamani");
        doctor.Especialidad.Should().Be("Ginecología");
        doctor.Celular.Should().Be("987654321");
        doctor.Correo.Should().Be("doctor@correo.com");
        doctor.FechaInicioContrato.Should().Be(fechaInicio);
        doctor.FechaFinContrato.Should().Be(fechaFin);
        doctor.Estado.Should().Be(EstadoDoctor.Inactivo);
        doctor.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void Colecciones_DebenPermitirAgregarElementos()
    {
        // Arrange
        var doctor = new Doctor();

        // Act
        doctor.Horarios.Add(new HorarioDoctor());
        doctor.Citas.Add(new Cita());
        doctor.Atenciones.Add(new Atencion());

        // Assert
        doctor.Horarios.Should().HaveCount(1);
        doctor.Citas.Should().HaveCount(1);
        doctor.Atenciones.Should().HaveCount(1);
    }

    [Fact]
    public void Estado_DebePoderCambiar()
    {
        // Arrange
        var doctor = new Doctor();

        // Act
        doctor.Estado = EstadoDoctor.Inactivo;
        doctor.Estado = EstadoDoctor.Eliminado;

        // Assert
        doctor.Estado.Should().Be(EstadoDoctor.Eliminado);
    }
}