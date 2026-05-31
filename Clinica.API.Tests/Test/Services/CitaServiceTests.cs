using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Citas;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class CitaServiceTests
{
    private readonly ICitaRepository _citaRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IServicioClinicoRepository _servicioRepository;
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _detalleRepository;
    private readonly IUsuarioActualService _usuarioActualService;

    private readonly ICitaService _service;

    public CitaServiceTests()
    {
        _citaRepository = Substitute.For<ICitaRepository>();
        _pacienteRepository = Substitute.For<IPacienteRepository>();
        _doctorRepository = Substitute.For<IDoctorRepository>();
        _servicioRepository = Substitute.For<IServicioClinicoRepository>();
        _historialRepository = Substitute.For<IHistorialClinicoRepository>();
        _detalleRepository = Substitute.For<IHistorialDetalleRepository>();
        _usuarioActualService = Substitute.For<IUsuarioActualService>();

        _service = new CitaService(
            _citaRepository,
            _pacienteRepository,
            _doctorRepository,
            _servicioRepository,
            _historialRepository,
            _detalleRepository,
            _usuarioActualService);
    }

    [Fact]
    public async Task ObtenerTodasAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var citas = new List<Cita>
        {
            CrearCitaEntidad(),
            CrearCitaEntidad()
        };

        _citaRepository.ObtenerTodasConRelacionesAsync().Returns(citas);

        // Act
        var resultado = (await _service.ObtenerTodasAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(citas[0].Id);
        resultado[0].CodigoCita.Should().Be(citas[0].CodigoCita);
        resultado[0].PacienteId.Should().Be(citas[0].PacienteId);
        resultado[0].DoctorId.Should().Be(citas[0].DoctorId);
        resultado[0].ServicioClinicoId.Should().Be(citas[0].ServicioClinicoId);
        resultado[0].PacienteNombre.Should().Be($"{citas[0].Paciente.Nombres} {citas[0].Paciente.Apellidos}");
        resultado[0].DoctorNombre.Should().Be($"{citas[0].Doctor.Nombres} {citas[0].Doctor.Apellidos}");
        resultado[0].ServicioNombre.Should().Be(citas[0].ServicioClinico.Nombre);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var cita = CrearCitaEntidad();
        _citaRepository.ObtenerPorIdConRelacionesAsync(cita.Id).Returns(cita);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(cita.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(cita.Id);
        resultado.CodigoCita.Should().Be(cita.CodigoCita);
        resultado.PacienteNombre.Should().Be($"{cita.Paciente.Nombres} {cita.Paciente.Apellidos}");
        resultado.DoctorNombre.Should().Be($"{cita.Doctor.Nombres} {cita.Doctor.Apellidos}");
        resultado.ServicioNombre.Should().Be(cita.ServicioClinico.Nombre);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _citaRepository.GetByIdAsync(id).Returns((Cita?)null);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var citas = new List<Cita> { CrearCitaEntidad(pacienteId: pacienteId) };

        _citaRepository.ObtenerPorPacienteAsync(pacienteId).Returns(citas);

        // Act
        var resultado = (await _service.ObtenerPorPacienteAsync(pacienteId)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].PacienteId.Should().Be(pacienteId);
    }

    [Fact]
    public async Task ObtenerPorDoctorAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var citas = new List<Cita> { CrearCitaEntidad(doctorId: doctorId) };

        _citaRepository.ObtenerPorDoctorAsync(doctorId).Returns(citas);

        // Act
        var resultado = (await _service.ObtenerPorDoctorAsync(doctorId)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].DoctorId.Should().Be(doctorId);
    }

    [Fact]
    public async Task CrearAsync_SiFechaEsPasada_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new CrearCitaDto
        {
            PacienteId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Control"
        };

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede programar una cita en una fecha pasada.");
    }

    [Fact]
    public async Task CrearAsync_SiHoraFinNoEsMayor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new CrearCitaDto
        {
            PacienteId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            HoraInicio = new TimeOnly(10, 0),
            HoraFin = new TimeOnly(10, 0),
            Motivo = "Control"
        };

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La hora de fin debe ser mayor que la hora de inicio.");
    }

    [Fact]
    public async Task CrearAsync_SiPacienteNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = CrearDtoValido();

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns((Paciente?)null);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task CrearAsync_SiDoctorNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var paciente = CrearPaciente(dto.PacienteId);

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns((Doctor?)null);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Doctor no encontrado.");
    }

    [Fact]
    public async Task CrearAsync_SiServicioNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var paciente = CrearPaciente(dto.PacienteId);
        var doctor = CrearDoctor(dto.DoctorId);

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns((ServicioClinico?)null);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Servicio clínico no encontrado.");
    }

    [Fact]
    public async Task CrearAsync_SiExisteCruceHorario_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var paciente = CrearPaciente(dto.PacienteId);
        var doctor = CrearDoctor(dto.DoctorId);
        var servicio = CrearServicio(dto.ServicioClinicoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);

        _citaRepository.ExisteInterferenciaHorarioAsync(
            dto.DoctorId,
            dto.Fecha,
            dto.HoraInicio,
            dto.HoraFin,
            Arg.Any<Guid?>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El doctor ya tiene una cita en ese horario.");

        await _citaRepository.DidNotReceive().AddAsync(Arg.Any<Cita>());
        await _citaRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task CrearAsync_SiNoExisteHistorial_DebeCrearCitaYGuardar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();
        var paciente = CrearPaciente(dto.PacienteId);
        var doctor = CrearDoctor(dto.DoctorId);
        var servicio = CrearServicio(dto.ServicioClinicoId);

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);
        _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId).Returns((HistorialClinico?)null);
        _citaRepository.ExisteInterferenciaHorarioAsync(dto.DoctorId, dto.Fecha, dto.HoraInicio, dto.HoraFin).Returns(false);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _citaRepository.Received(1).AddAsync(Arg.Is<Cita>(c =>
            c.PacienteId == dto.PacienteId &&
            c.DoctorId == dto.DoctorId &&
            c.ServicioClinicoId == dto.ServicioClinicoId &&
            c.HorarioDoctorId == dto.HorarioDoctorId &&
            c.Fecha == dto.Fecha &&
            c.HoraInicio == dto.HoraInicio &&
            c.HoraFin == dto.HoraFin &&
            c.Motivo == dto.Motivo &&
            c.Observaciones == dto.Observaciones &&
            c.Estado == EstadoCita.Pendiente &&
            c.UsuarioRegistroId == usuarioId &&
            !string.IsNullOrWhiteSpace(c.CodigoCita) &&
            c.CodigoCita.Contains("-CIT-") &&
            c.CodigoCita.EndsWith(paciente.DNI)));

        await _detalleRepository.DidNotReceive().AddAsync(Arg.Any<HistorialDetalle>());
        await _citaRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CrearAsync_SiExisteHistorial_DebeCrearCitaYDetalleHistorial()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();
        var paciente = CrearPaciente(dto.PacienteId);
        var doctor = CrearDoctor(dto.DoctorId);
        var servicio = CrearServicio(dto.ServicioClinicoId);
        var historial = new HistorialClinico { Id = Guid.NewGuid(), PacienteId = dto.PacienteId };

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.GetByIdAsync(dto.PacienteId).Returns(paciente);
        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);
        _servicioRepository.GetByIdAsync(dto.ServicioClinicoId).Returns(servicio);
        _historialRepository.ObtenerPorPacienteAsync(dto.PacienteId).Returns(historial);
        _citaRepository.ExisteInterferenciaHorarioAsync(dto.DoctorId, dto.Fecha, dto.HoraInicio, dto.HoraFin).Returns(false);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _detalleRepository.Received(1).AddAsync(Arg.Is<HistorialDetalle>(d =>
            d.HistorialClinicoId == historial.Id &&
            d.TipoMovimiento == TipoMovimientoHistorial.CitaProgramada &&
            d.Titulo == "Cita programada" &&
            d.Descripcion.Contains(servicio.Nombre) &&
            d.UsuarioId == usuarioId &&
            !string.IsNullOrWhiteSpace(d.CodigoDetalle) &&
            d.CodigoDetalle.Contains(servicio.CodigoServicio) &&
            d.CodigoDetalle.EndsWith(paciente.DNI)));

        await _citaRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ReprogramarAsync_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ReprogramarCitaDto
        {
            DoctorId = Guid.NewGuid(),
            NuevaFecha = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            NuevaHoraInicio = new TimeOnly(8, 0),
            NuevaHoraFin = new TimeOnly(8, 30),
            MotivoReprogramacion = "Cambio de horario"
        };

        _citaRepository.GetByIdAsync(id).Returns((Cita?)null);

        // Act
        Func<Task> act = async () => await _service.ReprogramarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Cita no encontrada.");
    }

    [Fact]
    public async Task ReprogramarAsync_SiExisteCruce_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cita = CrearCitaEntidad(id: id);

        var dto = new ReprogramarCitaDto
        {
            DoctorId = Guid.NewGuid(),
            NuevaFecha = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            NuevaHoraInicio = new TimeOnly(10, 0),
            NuevaHoraFin = new TimeOnly(10, 30),
            MotivoReprogramacion = "Cambio de horario"
        };

        _citaRepository.GetByIdAsync(id).Returns(cita);
        _citaRepository.ExisteInterferenciaHorarioAsync(dto.DoctorId, dto.NuevaFecha, dto.NuevaHoraInicio, dto.NuevaHoraFin, id)
            .Returns(true);

        // Act
        Func<Task> act = async () => await _service.ReprogramarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("El doctor ya tiene una cita en ese nuevo horario.");

        _citaRepository.DidNotReceive().Update(Arg.Any<Cita>());
    }

    [Fact]
    public async Task ReprogramarAsync_SiFechaEsPasada_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cita = CrearCitaEntidad(id: id);

        var dto = new ReprogramarCitaDto
        {
            DoctorId = Guid.NewGuid(),
            NuevaFecha = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            NuevaHoraInicio = new TimeOnly(10, 0),
            NuevaHoraFin = new TimeOnly(10, 30),
            MotivoReprogramacion = "Cambio de horario"
        };

        _citaRepository.GetByIdAsync(id).Returns(cita);
        _citaRepository.ExisteInterferenciaHorarioAsync(dto.DoctorId, dto.NuevaFecha, dto.NuevaHoraInicio, dto.NuevaHoraFin, id)
            .Returns(false);

        // Act
        Func<Task> act = async () => await _service.ReprogramarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede reprogramar una cita en una fecha pasada.");
    }

    [Fact]
    public async Task ReprogramarAsync_SiHoraFinNoEsMayor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cita = CrearCitaEntidad(id: id);

        var dto = new ReprogramarCitaDto
        {
            DoctorId = Guid.NewGuid(),
            NuevaFecha = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            NuevaHoraInicio = new TimeOnly(10, 0),
            NuevaHoraFin = new TimeOnly(9, 45),
            MotivoReprogramacion = "Cambio de horario"
        };

        _citaRepository.GetByIdAsync(id).Returns(cita);
        _citaRepository.ExisteInterferenciaHorarioAsync(dto.DoctorId, dto.NuevaFecha, dto.NuevaHoraInicio, dto.NuevaHoraFin, id)
            .Returns(false);

        // Act
        Func<Task> act = async () => await _service.ReprogramarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La hora de fin debe ser mayor que la hora de inicio.");
    }

    [Fact]
    public async Task ReprogramarAsync_SiTodoVaBien_DebeActualizarCitaYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cita = CrearCitaEntidad(id: id);

        var dto = new ReprogramarCitaDto
        {
            DoctorId = Guid.NewGuid(),
            HorarioDoctorId = Guid.NewGuid(),
            NuevaFecha = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            NuevaHoraInicio = new TimeOnly(11, 0),
            NuevaHoraFin = new TimeOnly(11, 45),
            MotivoReprogramacion = "Doctor solicitó cambio"
        };

        _citaRepository.GetByIdAsync(id).Returns(cita);
        _citaRepository.ExisteInterferenciaHorarioAsync(dto.DoctorId, dto.NuevaFecha, dto.NuevaHoraInicio, dto.NuevaHoraFin, id)
            .Returns(false);

        // Act
        await _service.ReprogramarAsync(id, dto);

        // Assert
        cita.DoctorId.Should().Be(dto.DoctorId);
        cita.HorarioDoctorId.Should().Be(dto.HorarioDoctorId);
        cita.Fecha.Should().Be(dto.NuevaFecha);
        cita.HoraInicio.Should().Be(dto.NuevaHoraInicio);
        cita.HoraFin.Should().Be(dto.NuevaHoraFin);
        cita.Estado.Should().Be(EstadoCita.Reprogramada);
        cita.Observaciones.Should().Be(dto.MotivoReprogramacion);

        _citaRepository.Received(1).Update(cita);
        await _citaRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CancelarAsync_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CancelarCitaDto { MotivoCancelacion = "Paciente no asistirá" };

        _citaRepository.GetByIdAsync(id).Returns((Cita?)null);

        // Act
        Func<Task> act = async () => await _service.CancelarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Cita no encontrada.");
    }

    [Fact]
    public async Task CancelarAsync_SiExiste_DebeCambiarEstadoYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cita = CrearCitaEntidad(id: id);
        var dto = new CancelarCitaDto { MotivoCancelacion = "Paciente solicitó cancelación" };

        _citaRepository.GetByIdAsync(id).Returns(cita);

        // Act
        await _service.CancelarAsync(id, dto);

        // Assert
        cita.Estado.Should().Be(EstadoCita.Cancelada);
        cita.Observaciones.Should().Be(dto.MotivoCancelacion);

        _citaRepository.Received(1).Update(cita);
        await _citaRepository.Received(1).SaveChangesAsync();
    }

    private static CrearCitaDto CrearDtoValido()
    {
        return new CrearCitaDto
        {
            PacienteId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            HorarioDoctorId = Guid.NewGuid(),
            Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Control prenatal",
            Observaciones = "Sin observaciones"
        };
    }

    private static Cita CrearCitaEntidad(
        Guid? id = null,
        Guid? pacienteId = null,
        Guid? doctorId = null,
        Guid? servicioId = null)
    {
        var pId = pacienteId ?? Guid.NewGuid();
        var dId = doctorId ?? Guid.NewGuid();
        var sId = servicioId ?? Guid.NewGuid();

        return new Cita
        {
            Id = id ?? Guid.NewGuid(),
            CodigoCita = "ABCDE-CIT-2026-12345678",
            PacienteId = pId,
            Paciente = CrearPaciente(pId),
            DoctorId = dId,
            Doctor = CrearDoctor(dId),
            ServicioClinicoId = sId,
            ServicioClinico = CrearServicio(sId),
            Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(8, 30),
            Motivo = "Control",
            Observaciones = "Obs",
            Estado = EstadoCita.Pendiente,
            FechaRegistro = DateTime.UtcNow
        };
    }

    private static Paciente CrearPaciente(Guid id)
    {
        return new Paciente
        {
            Id = id,
            DNI = "12345678",
            Nombres = "Ana",
            Apellidos = "Quispe"
        };
    }

    private static Doctor CrearDoctor(Guid id)
    {
        return new Doctor
        {
            Id = id,
            Nombres = "Luis",
            Apellidos = "Mamani"
        };
    }

    private static ServicioClinico CrearServicio(Guid id)
    {
        return new ServicioClinico
        {
            Id = id,
            CodigoServicio = "CONOBS",
            Nombre = "Consulta obstétrica"
        };
    }
    
    [Fact]
    public async Task ObtenerTodasAsync_CuandoPacienteEsNull_DebeRetornarNombreVacio()
    {
        // Arrange
        var cita = CrearCitaEntidad();
        cita.Paciente = null; // anulamos la navegación
        _citaRepository.ObtenerTodasConRelacionesAsync().Returns(new List<Cita> { cita });

        // Act
        var resultado = (await _service.ObtenerTodasAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].PacienteNombre.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerTodasAsync_CuandoDoctorEsNull_DebeRetornarNombreVacio()
    {
        var cita = CrearCitaEntidad();
        cita.Doctor = null;
        _citaRepository.ObtenerTodasConRelacionesAsync().Returns(new List<Cita> { cita });

        var resultado = (await _service.ObtenerTodasAsync()).ToList();
        resultado[0].DoctorNombre.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerTodasAsync_CuandoServicioClinicoEsNull_DebeRetornarNombreVacio()
    {
        var cita = CrearCitaEntidad();
        cita.ServicioClinico = null;
        _citaRepository.ObtenerTodasConRelacionesAsync().Returns(new List<Cita> { cita });

        var resultado = (await _service.ObtenerTodasAsync()).ToList();
        resultado[0].ServicioNombre.Should().BeEmpty();
    }
}