using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Horarios;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class HorarioDoctorServiceTests
{
    private readonly IHorarioDoctorRepository _horarioRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IHorarioDoctorService _service;
    private ICitaRepository _citaRepository;

    public HorarioDoctorServiceTests()
    {
        _horarioRepository = Substitute.For<IHorarioDoctorRepository>();
        _doctorRepository = Substitute.For<IDoctorRepository>();
        _citaRepository = Substitute.For<ICitaRepository>();
        _service = new HorarioDoctorService(_horarioRepository, _doctorRepository, _citaRepository);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var horarios = new List<HorarioDoctor>
        {
            CrearHorarioEntidad(),
            CrearHorarioEntidad()
        };

        _horarioRepository.ObtenerTodosConDoctorAsync().Returns(horarios);

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(horarios[0].Id);
        resultado[0].DoctorId.Should().Be(horarios[0].DoctorId);
        resultado[0].DoctorNombre.Should().Be($"{horarios[0].Doctor.Nombres} {horarios[0].Doctor.Apellidos}");
        resultado[0].DiaSemana.Should().Be(horarios[0].DiaSemana);
        resultado[0].HoraInicio.Should().Be(horarios[0].HoraInicio);
        resultado[0].HoraFin.Should().Be(horarios[0].HoraFin);
        resultado[0].FechaInicioVigencia.Should().Be(horarios[0].FechaInicioVigencia);
        resultado[0].FechaFinVigencia.Should().Be(horarios[0].FechaFinVigencia);
        resultado[0].Activo.Should().Be(horarios[0].Activo);
    }

    [Fact]
    public async Task ObtenerPorDoctorAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var horarios = new List<HorarioDoctor>
        {
            CrearHorarioEntidad(doctorId: doctorId)
        };

        _horarioRepository.ObtenerPorDoctorAsync(doctorId).Returns(horarios);

        // Act
        var resultado = (await _service.ObtenerPorDoctorAsync(doctorId)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].DoctorId.Should().Be(doctorId);
    }

    [Fact]
    public async Task CrearAsync_SiDoctorNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = CrearDtoValido();

        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns((Doctor?)null);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Doctor no encontrado.");

        await _horarioRepository.DidNotReceive().AddAsync(Arg.Any<HorarioDoctor>());
    }

    [Fact]
    public async Task CrearAsync_SiHoraFinNoEsMayor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var doctor = CrearDoctor(dto.DoctorId);
        dto.HoraFin = dto.HoraInicio;

        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La hora de fin debe ser mayor que la hora de inicio.");
    }

    [Fact]
    public async Task CrearAsync_SiFechaFinEsMenor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var doctor = CrearDoctor(dto.DoctorId);
        dto.FechaFinVigencia = dto.FechaInicioVigencia.AddDays(-1);

        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La fecha de fin de vigencia no puede ser menor que la fecha de inicio.");
    }

    [Fact]
    public async Task CrearAsync_SiTodoEsValido_DebeCrearHorarioYGuardar()
    {
        // Arrange
        var dto = CrearDtoValido();
        var doctor = CrearDoctor(dto.DoctorId);

        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _horarioRepository.Received(1).AddAsync(Arg.Is<HorarioDoctor>(h =>
            h.DoctorId == dto.DoctorId &&
            h.DiaSemana == dto.DiaSemana &&
            h.HoraInicio == dto.HoraInicio &&
            h.HoraFin == dto.HoraFin &&
            h.FechaInicioVigencia == dto.FechaInicioVigencia &&
            h.FechaFinVigencia == dto.FechaFinVigencia &&
            h.Activo));

        await _horarioRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ActualizarAsync_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = CrearEditarDtoValido();

        _horarioRepository.GetByIdAsync(id).Returns((HorarioDoctor?)null);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Horario no encontrado.");
    }

    [Fact]
    public async Task ActualizarAsync_SiHoraFinNoEsMayor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var horario = CrearHorarioEntidad(id: id);
        var dto = CrearEditarDtoValido();
        dto.HoraFin = dto.HoraInicio;

        _horarioRepository.GetByIdAsync(id).Returns(horario);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La hora de fin debe ser mayor que la hora de inicio.");
    }

    [Fact]
    public async Task ActualizarAsync_SiFechaFinEsMenor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var horario = CrearHorarioEntidad(id: id);
        var dto = CrearEditarDtoValido();
        dto.FechaFinVigencia = dto.FechaInicioVigencia.AddDays(-1);

        _horarioRepository.GetByIdAsync(id).Returns(horario);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La fecha de fin de vigencia no puede ser menor que la fecha de inicio.");
    }

    [Fact]
    public async Task ActualizarAsync_SiExiste_DebeActualizarYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var horario = CrearHorarioEntidad(id: id);
        var dto = CrearEditarDtoValido();

        _horarioRepository.GetByIdAsync(id).Returns(horario);

        // Act
        await _service.ActualizarAsync(id, dto);

        // Assert
        horario.DiaSemana.Should().Be(dto.DiaSemana);
        horario.HoraInicio.Should().Be(dto.HoraInicio);
        horario.HoraFin.Should().Be(dto.HoraFin);
        horario.FechaInicioVigencia.Should().Be(dto.FechaInicioVigencia);
        horario.FechaFinVigencia.Should().Be(dto.FechaFinVigencia);
        horario.Activo.Should().Be(dto.Activo);

        _horarioRepository.Received(1).Update(horario);
        await _horarioRepository.Received(1).SaveChangesAsync();
    }

    private static CrearHorarioDoctorDto CrearDtoValido()
    {
        return new CrearHorarioDoctorDto
        {
            DoctorId = Guid.NewGuid(),
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(12, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.Today),
            FechaFinVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
        };
    }

    private static EditarHorarioDoctorDto CrearEditarDtoValido()
    {
        return new EditarHorarioDoctorDto
        {
            DiaSemana = DayOfWeek.Wednesday,
            HoraInicio = new TimeOnly(14, 0),
            HoraFin = new TimeOnly(18, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            FechaFinVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(40)),
            Activo = false
        };
    }

    private static HorarioDoctor CrearHorarioEntidad(Guid? id = null, Guid? doctorId = null)
    {
        var dId = doctorId ?? Guid.NewGuid();

        return new HorarioDoctor
        {
            Id = id ?? Guid.NewGuid(),
            DoctorId = dId,
            Doctor = CrearDoctor(dId),
            DiaSemana = DayOfWeek.Tuesday,
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.Today),
            FechaFinVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            Activo = true
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
    
    [Fact]
    public async Task CrearAsync_CuandoFechaFinEsNull_DebeCrearHorario()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.FechaFinVigencia = null; // cubre HasValue == false
        var doctor = CrearDoctor(dto.DoctorId);

        _doctorRepository.GetByIdAsync(dto.DoctorId).Returns(doctor);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();
        await _horarioRepository.Received(1).AddAsync(Arg.Is<HorarioDoctor>(h =>
            h.FechaFinVigencia == null));
        await _horarioRepository.Received(1).SaveChangesAsync();
    }
    [Fact]
    public async Task ActualizarAsync_CuandoFechaFinEsNull_DebeActualizar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var horario = CrearHorarioEntidad(id: id);
        var dto = CrearEditarDtoValido();
        dto.FechaFinVigencia = null; // cubre HasValue == false

        _horarioRepository.GetByIdAsync(id).Returns(horario);

        // Act
        await _service.ActualizarAsync(id, dto);

        // Assert
        horario.FechaFinVigencia.Should().BeNull();
        _horarioRepository.Received(1).Update(horario);
        await _horarioRepository.Received(1).SaveChangesAsync();
    }
    [Fact]
    public async Task ObtenerTodosAsync_CuandoDoctorEsNull_DebeRetornarNombreVacio()
    {
        var horario = CrearHorarioEntidad();
        horario.Doctor = null;

        _horarioRepository.ObtenerTodosConDoctorAsync().Returns(new List<HorarioDoctor> { horario });

        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        resultado[0].DoctorNombre.Should().BeEmpty();
    }
    [Fact]
    public async Task ObtenerMatrizSemanalAsync_ConDoctorExistente_RetornaMatrizConFilas()
    {
        // Arrange
        var doctor = CrearDoctor(Guid.NewGuid());
        var fechaRef = new DateOnly(2026, 5, 12); // martes

        _doctorRepository.GetByIdAsync(doctor.Id).Returns(doctor);
        _horarioRepository.ObtenerPorDoctorAsync(doctor.Id)
            .Returns(new List<HorarioDoctor>()); // sin plantillas -> todas celdas "FueraHorario"
        _citaRepository.ObtenerCitasParaRecordatorioAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Cita>());

        // Act
        var matriz = await _service.ObtenerMatrizSemanalAsync(doctor.Id, fechaRef);

        // Assert
        matriz.DoctorId.Should().Be(doctor.Id);
        matriz.FechaInicioSemana.DayOfWeek.Should().Be(DayOfWeek.Monday);
        matriz.FechaFinSemana.DayOfWeek.Should().Be(DayOfWeek.Sunday);
        matriz.Filas.Should().NotBeEmpty();
        // Todas las celdas deben estar en "FueraHorario" porque no hay plantillas
        matriz.Filas.SelectMany(f => f.CeldasDias).Should()
            .OnlyContain(c => c.Estado == "FueraHorario");
    }
}