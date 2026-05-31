using System.Reflection;
using Clinica.API.Configurations;
using Clinica.API.Services;
using Clinica.API.Services.Background;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class RecordatorioCitasBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_SiServicioEstaDeshabilitado_DebeFinalizarSinProcesar()
    {
        // Arrange
        var citaRepository = Substitute.For<ICitaRepository>();
        var notificacionRepository = Substitute.For<INotificacionCitaRepository>();
        var whatsAppService = Substitute.For<INotificacionWhatsAppService>();
        var logger = Substitute.For<ILogger<RecordatorioCitasBackgroundService>>();

        var serviceProvider = CrearServiceProvider(citaRepository, notificacionRepository, whatsAppService);
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var options = Options.Create(new WhatsAppOptions
        {
            Enabled = false,
            ReminderHoursBefore = 24,
            CheckIntervalMinutes = 5,
            MaxIntentos = 3
        });

        var service = new TestableRecordatorioCitasBackgroundService(scopeFactory, options, logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.ExecutePublicAsync(cts.Token);

        // Assert
        await citaRepository.DidNotReceive()
            .ObtenerCitasParaRecordatorioAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>());
        
    }

    [Fact]
    public async Task ProcesarRecordatoriosAsync_SiHayCitaSinRecordatorio_DebeCrearNotificacionPendiente()
    {
        // Arrange
        var citaRepository = Substitute.For<ICitaRepository>();
        var notificacionRepository = Substitute.For<INotificacionCitaRepository>();
        var whatsAppService = Substitute.For<INotificacionWhatsAppService>();
        var logger = Substitute.For<ILogger<RecordatorioCitasBackgroundService>>();

        var cita = CrearCita();
        citaRepository.ObtenerCitasParaRecordatorioAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new[] { cita });

        notificacionRepository.ExisteRecordatorioParaCitaAsync(cita.Id).Returns(false);
        notificacionRepository.ObtenerPendientesAsync(Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(Enumerable.Empty<NotificacionCita>());

        var service = CrearServicio(citaRepository, notificacionRepository, whatsAppService, logger);

        // Act
        await InvocarProcesarRecordatoriosAsync(service);

        // Assert
        await notificacionRepository.Received(1).AddAsync(Arg.Is<NotificacionCita>(n =>
            n.CitaId == cita.Id &&
            n.PacienteId == cita.PacienteId &&
            n.TelefonoDestino == cita.Paciente.Celular &&
            n.Canal == CanalNotificacion.WhatsApp &&
            n.Estado == EstadoNotificacion.Pendiente &&
            n.Intentos == 0 &&
            !string.IsNullOrWhiteSpace(n.Mensaje)));
        
    }

    [Fact]
    public async Task ProcesarRecordatoriosAsync_SiYaExisteRecordatorio_NoDebeCrearOtro()
    {
        // Arrange
        var citaRepository = Substitute.For<ICitaRepository>();
        var notificacionRepository = Substitute.For<INotificacionCitaRepository>();
        var whatsAppService = Substitute.For<INotificacionWhatsAppService>();
        var logger = Substitute.For<ILogger<RecordatorioCitasBackgroundService>>();

        var cita = CrearCita();
        citaRepository.ObtenerCitasParaRecordatorioAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new[] { cita });

        notificacionRepository.ExisteRecordatorioParaCitaAsync(cita.Id).Returns(true);
        notificacionRepository.ObtenerPendientesAsync(Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(Enumerable.Empty<NotificacionCita>());

        var service = CrearServicio(citaRepository, notificacionRepository, whatsAppService, logger);

        // Act
        await InvocarProcesarRecordatoriosAsync(service);

        // Assert
        await notificacionRepository.DidNotReceive().AddAsync(Arg.Any<NotificacionCita>());
        
    }

    [Fact]
    public async Task ProcesarRecordatoriosAsync_SiHayPendienteYEnvioEsExitoso_DebeMarcarComoEnviado()
    {
        // Arrange
        var citaRepository = Substitute.For<ICitaRepository>();
        var notificacionRepository = Substitute.For<INotificacionCitaRepository>();
        var whatsAppService = Substitute.For<INotificacionWhatsAppService>();
        var logger = Substitute.For<ILogger<RecordatorioCitasBackgroundService>>();

        citaRepository.ObtenerCitasParaRecordatorioAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Enumerable.Empty<Cita>());

        var notificacion = new NotificacionCita
        {
            Id = Guid.NewGuid(),
            CitaId = Guid.NewGuid(),
            PacienteId = Guid.NewGuid(),
            TelefonoDestino = "51987654321",
            Mensaje = "Mensaje de prueba",
            Estado = EstadoNotificacion.Pendiente,
            Intentos = 0
        };

        notificacionRepository.ObtenerPendientesAsync(Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(new[] { notificacion });

        var service = CrearServicio(citaRepository, notificacionRepository, whatsAppService, logger);

        // Act
        await InvocarProcesarRecordatoriosAsync(service);

        // Assert
        await whatsAppService.Received(1)
            .EnviarMensajeAsync(notificacion.TelefonoDestino, notificacion.Mensaje, Arg.Any<CancellationToken>());

        notificacion.Intentos.Should().Be(1);
        notificacion.Estado.Should().Be(EstadoNotificacion.Enviado);
        notificacion.FechaEnvio.Should().NotBeNull();
        notificacion.Error.Should().BeNull();

        await notificacionRepository.Received(1).ActualizarAsync(notificacion);
        
    }

    [Fact]
    public async Task ProcesarRecordatoriosAsync_SiEnvioFallaYLlegaAlMaximo_DebeMarcarComoFallido()
    {
        // Arrange
        var citaRepository = Substitute.For<ICitaRepository>();
        var notificacionRepository = Substitute.For<INotificacionCitaRepository>();
        var whatsAppService = Substitute.For<INotificacionWhatsAppService>();
        var logger = Substitute.For<ILogger<RecordatorioCitasBackgroundService>>();

        citaRepository.ObtenerCitasParaRecordatorioAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Enumerable.Empty<Cita>());

        var notificacion = new NotificacionCita
        {
            Id = Guid.NewGuid(),
            CitaId = Guid.NewGuid(),
            PacienteId = Guid.NewGuid(),
            TelefonoDestino = "51987654321",
            Mensaje = "Mensaje de prueba",
            Estado = EstadoNotificacion.Pendiente,
            Intentos = 2
        };

        notificacionRepository.ObtenerPendientesAsync(Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(new[] { notificacion });

        whatsAppService
            .EnviarMensajeAsync(notificacion.TelefonoDestino, notificacion.Mensaje, Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("Error al enviar"));

        var service = CrearServicio(citaRepository, notificacionRepository, whatsAppService, logger);

        // Act
        await InvocarProcesarRecordatoriosAsync(service);

        // Assert
        notificacion.Intentos.Should().Be(3);
        notificacion.Estado.Should().Be(EstadoNotificacion.Fallido);
        notificacion.Error.Should().Be("Error al enviar");
        notificacion.FechaActualizacion.Should().NotBeNull();

        await notificacionRepository.Received(1).ActualizarAsync(notificacion);
        
    }

    [Fact]
    public async Task ConstruirMensaje_DebeGenerarTextoConDatosDeLaCita()
    {
        // Arrange
        var cita = CrearCita();

        // Act
        var mensaje = InvocarConstruirMensaje(cita);

        // Assert
        mensaje.Should().Contain("CLÍNICA SANTA MÓNICA");
        mensaje.Should().Contain("ANA QUISPE");
        mensaje.Should().Contain("LUIS MAMANI");
        mensaje.Should().Contain(cita.HoraInicio.ToString("HH:mm"));
        mensaje.Should().Contain("Recordatorio de cita");
        
    }

    private static RecordatorioCitasBackgroundService CrearServicio(
        ICitaRepository citaRepository,
        INotificacionCitaRepository notificacionRepository,
        INotificacionWhatsAppService whatsAppService,
        ILogger<RecordatorioCitasBackgroundService> logger)
    {
        var serviceProvider = CrearServiceProvider(citaRepository, notificacionRepository, whatsAppService);
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var options = Options.Create(new WhatsAppOptions
        {
            Enabled = true,
            ReminderHoursBefore = 24,
            CheckIntervalMinutes = 5,
            MaxIntentos = 3
        });

        return new RecordatorioCitasBackgroundService(scopeFactory, options, logger);
    }

    private static ServiceProvider CrearServiceProvider(
        ICitaRepository citaRepository,
        INotificacionCitaRepository notificacionRepository,
        INotificacionWhatsAppService whatsAppService)
    {
        var services = new ServiceCollection();

        services.AddSingleton(citaRepository);
        services.AddSingleton(notificacionRepository);
        services.AddSingleton(whatsAppService);

        return services.BuildServiceProvider();
    }

    private static async Task InvocarProcesarRecordatoriosAsync(RecordatorioCitasBackgroundService service)
    {
        var method = typeof(RecordatorioCitasBackgroundService)
            .GetMethod("ProcesarRecordatoriosAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        method.Should().NotBeNull();

        var task = (Task?)method!.Invoke(service, new object[] { CancellationToken.None });
        task.Should().NotBeNull();

        await task!;
    }

    private static string InvocarConstruirMensaje(Cita cita)
    {
        var method = typeof(RecordatorioCitasBackgroundService)
            .GetMethod("ConstruirMensaje", BindingFlags.Static | BindingFlags.NonPublic);

        method.Should().NotBeNull();

        var resultado = method!.Invoke(null, new object[] { cita });

        return resultado.Should().BeOfType<string>().Subject;
    }

    private static Cita CrearCita()
    {
        var fecha = new DateOnly(2026, 5, 21);
        var horaInicio = new TimeOnly(9, 30);

        return new Cita
        {
            Id = Guid.NewGuid(),
            PacienteId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            Fecha = fecha,
            HoraInicio = horaInicio,
            HoraFin = new TimeOnly(10, 0),
            Paciente = new Paciente
            {
                Id = Guid.NewGuid(),
                Nombres = "Ana",
                Apellidos = "Quispe",
                Celular = "51987654321"
            },
            Doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                Nombres = "Luis",
                Apellidos = "Mamani"
            }
        };
    }

    private sealed class TestableRecordatorioCitasBackgroundService : RecordatorioCitasBackgroundService
    {
        public TestableRecordatorioCitasBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<WhatsAppOptions> options,
            ILogger<RecordatorioCitasBackgroundService> logger)
            : base(scopeFactory, options, logger)
        {
        }

        public Task ExecutePublicAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);
        }
    }
}