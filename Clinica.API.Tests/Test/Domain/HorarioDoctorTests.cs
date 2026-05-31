using Clinica.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class HorarioDoctorTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var horario = new HorarioDoctor();

        // Assert
        horario.Id.Should().NotBeEmpty();
        horario.DoctorId.Should().BeEmpty();
        horario.Doctor.Should().BeNull();
        horario.DiaSemana.Should().Be(default);
        horario.HoraInicio.Should().Be(default);
        horario.HoraFin.Should().Be(default);
        horario.FechaInicioVigencia.Should().Be(default);
        horario.FechaFinVigencia.Should().BeNull();
        horario.Activo.Should().BeTrue();
        horario.Citas.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var fechaInicio = DateOnly.FromDateTime(DateTime.Today);
        var fechaFin = fechaInicio.AddDays(30);

        // Act
        var horario = new HorarioDoctor
        {
            DoctorId = doctorId,
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(12, 0),
            FechaInicioVigencia = fechaInicio,
            FechaFinVigencia = fechaFin,
            Activo = false
        };

        // Assert
        horario.DoctorId.Should().Be(doctorId);
        horario.DiaSemana.Should().Be(DayOfWeek.Monday);
        horario.HoraInicio.Should().Be(new TimeOnly(8, 0));
        horario.HoraFin.Should().Be(new TimeOnly(12, 0));
        horario.FechaInicioVigencia.Should().Be(fechaInicio);
        horario.FechaFinVigencia.Should().Be(fechaFin);
        horario.Activo.Should().BeFalse();
    }

    [Fact]
    public void ColeccionCitas_DebePermitirAgregarElementos()
    {
        // Arrange
        var horario = new HorarioDoctor();

        // Act
        horario.Citas.Add(new Cita());

        // Assert
        horario.Citas.Should().HaveCount(1);
    }

    [Fact]
    public void Activo_DebePoderCambiar()
    {
        // Arrange
        var horario = new HorarioDoctor();

        // Act
        horario.Activo = false;

        // Assert
        horario.Activo.Should().BeFalse();
    }
}