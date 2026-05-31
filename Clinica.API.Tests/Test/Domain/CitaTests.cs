using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class CitaTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var cita = new Cita();

        // Assert
        cita.Id.Should().NotBeEmpty();
        cita.CodigoCita.Should().BeEmpty();
        cita.PacienteId.Should().Be(Guid.Empty);
        cita.DoctorId.Should().Be(Guid.Empty);
        cita.ServicioClinicoId.Should().Be(Guid.Empty);
        cita.HorarioDoctorId.Should().BeNull();

        cita.Fecha.Should().Be(default);
        cita.HoraInicio.Should().Be(default);
        cita.HoraFin.Should().Be(default);

        cita.Motivo.Should().BeEmpty();
        cita.Observaciones.Should().BeNull();

        cita.Estado.Should().Be(EstadoCita.Pendiente);
        cita.FechaRegistro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        cita.UsuarioRegistroId.Should().BeNull();
        cita.UsuarioRegistro.Should().BeNull();
        cita.Atencion.Should().BeNull();

        cita.Pagos.Should().NotBeNull().And.BeEmpty();
        cita.HistorialDetalles.Should().NotBeNull().And.BeEmpty();
        cita.Comprobantes.Should().NotBeNull().And.BeEmpty();
        cita.Notificaciones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Propiedades_DebenPermitirAsignarValoresCorrectamente()
    {
        // Arrange
        var citaId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var servicioId = Guid.NewGuid();
        var horarioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var fechaRegistro = DateTime.UtcNow.AddMinutes(-10);

        // Act
        var cita = new Cita
        {
            Id = citaId,
            CodigoCita = "ABCDE-CIT-2026-12345678",
            PacienteId = pacienteId,
            DoctorId = doctorId,
            ServicioClinicoId = servicioId,
            HorarioDoctorId = horarioId,
            Fecha = new DateOnly(2026, 5, 20),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Control prenatal",
            Observaciones = "Paciente refiere molestias leves",
            Estado = EstadoCita.Confirmada,
            FechaRegistro = fechaRegistro,
            UsuarioRegistroId = usuarioId
        };

        // Assert
        cita.Id.Should().Be(citaId);
        cita.CodigoCita.Should().Be("ABCDE-CIT-2026-12345678");
        cita.PacienteId.Should().Be(pacienteId);
        cita.DoctorId.Should().Be(doctorId);
        cita.ServicioClinicoId.Should().Be(servicioId);
        cita.HorarioDoctorId.Should().Be(horarioId);
        cita.Fecha.Should().Be(new DateOnly(2026, 5, 20));
        cita.HoraInicio.Should().Be(new TimeOnly(9, 0));
        cita.HoraFin.Should().Be(new TimeOnly(9, 30));
        cita.Motivo.Should().Be("Control prenatal");
        cita.Observaciones.Should().Be("Paciente refiere molestias leves");
        cita.Estado.Should().Be(EstadoCita.Confirmada);
        cita.FechaRegistro.Should().Be(fechaRegistro);
        cita.UsuarioRegistroId.Should().Be(usuarioId);
    }

    [Fact]
    public void Colecciones_DebenSerModificables()
    {
        // Arrange
        var cita = new Cita();

        // Act
        cita.Pagos.Add(new Pago());
        cita.HistorialDetalles.Add(new HistorialDetalle());
        cita.Comprobantes.Add(new Comprobante());
        cita.Notificaciones.Add(new NotificacionCita());

        // Assert
        cita.Pagos.Should().HaveCount(1);
        cita.HistorialDetalles.Should().HaveCount(1);
        cita.Comprobantes.Should().HaveCount(1);
        cita.Notificaciones.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(EstadoCita.Pendiente)]
    [InlineData(EstadoCita.Confirmada)]
    [InlineData(EstadoCita.Reprogramada)]
    [InlineData(EstadoCita.Cancelada)]
    [InlineData(EstadoCita.Atendida)]
    [InlineData(EstadoCita.NoAsistio)]
    [InlineData(EstadoCita.Eliminada)]
    public void Estado_DebeAceptarTodosLosValoresDelEnum(EstadoCita estado)
    {
        // Arrange
        var cita = new Cita();

        // Act
        cita.Estado = estado;

        // Assert
        cita.Estado.Should().Be(estado);
    }
}