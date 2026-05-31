using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class NotificacionCitaTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var notificacion = new NotificacionCita();

        // Assert
        notificacion.Id.Should().NotBeEmpty();

        notificacion.CitaId.Should().BeEmpty();
        notificacion.Cita.Should().BeNull();

        notificacion.PacienteId.Should().BeEmpty();
        notificacion.Paciente.Should().BeNull();

        notificacion.TelefonoDestino.Should().BeEmpty();
        notificacion.Canal.Should().Be(CanalNotificacion.WhatsApp);
        notificacion.Mensaje.Should().BeEmpty();

        notificacion.FechaProgramadaEnvio.Should().Be(default);
        notificacion.FechaEnvio.Should().BeNull();

        notificacion.Estado.Should().Be(EstadoNotificacion.Pendiente);
        notificacion.Intentos.Should().Be(0);
        notificacion.Error.Should().BeNull();

        notificacion.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        notificacion.FechaActualizacion.Should().BeNull();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var citaId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
        var fechaProgramada = new DateTime(2026, 5, 20, 8, 0, 0, DateTimeKind.Utc);
        var fechaEnvio = new DateTime(2026, 5, 20, 8, 1, 0, DateTimeKind.Utc);
        var fechaActualizacion = new DateTime(2026, 5, 20, 8, 2, 0, DateTimeKind.Utc);

        // Act
        var notificacion = new NotificacionCita
        {
            CitaId = citaId,
            PacienteId = pacienteId,
            TelefonoDestino = "51987654321",
            Canal = CanalNotificacion.WhatsApp,
            Mensaje = "Recordatorio de cita",
            FechaProgramadaEnvio = fechaProgramada,
            FechaEnvio = fechaEnvio,
            Estado = EstadoNotificacion.Enviado,
            Intentos = 1,
            Error = null,
            FechaActualizacion = fechaActualizacion
        };

        // Assert
        notificacion.CitaId.Should().Be(citaId);
        notificacion.PacienteId.Should().Be(pacienteId);
        notificacion.TelefonoDestino.Should().Be("51987654321");
        notificacion.Canal.Should().Be(CanalNotificacion.WhatsApp);
        notificacion.Mensaje.Should().Be("Recordatorio de cita");
        notificacion.FechaProgramadaEnvio.Should().Be(fechaProgramada);
        notificacion.FechaEnvio.Should().Be(fechaEnvio);
        notificacion.Estado.Should().Be(EstadoNotificacion.Enviado);
        notificacion.Intentos.Should().Be(1);
        notificacion.FechaActualizacion.Should().Be(fechaActualizacion);
    }

    [Fact]
    public void Estado_DebePoderCambiar()
    {
        // Arrange
        var notificacion = new NotificacionCita();

        // Act
        notificacion.Estado = EstadoNotificacion.Enviado;
        notificacion.Estado = EstadoNotificacion.Fallido;

        // Assert
        notificacion.Estado.Should().Be(EstadoNotificacion.Fallido);
    }
}