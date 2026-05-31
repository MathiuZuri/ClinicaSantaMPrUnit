using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class DoctorServiceTests
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    private readonly IDoctorService _service;

    public DoctorServiceTests()
    {
        _doctorRepository = Substitute.For<IDoctorRepository>();
        _usuarioActualService = Substitute.For<IUsuarioActualService>();

        _service = new DoctorService(_doctorRepository, _usuarioActualService);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var doctores = new List<Doctor>
        {
            CrearDoctorEntidad(),
            CrearDoctorEntidad()
        };

        _doctorRepository.GetAllAsync().Returns(doctores);

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(doctores[0].Id);
        resultado[0].CodigoDoctor.Should().Be(doctores[0].CodigoDoctor);
        resultado[0].CMP.Should().Be(doctores[0].CMP);
        resultado[0].Nombres.Should().Be(doctores[0].Nombres);
        resultado[0].Apellidos.Should().Be(doctores[0].Apellidos);
        resultado[0].Especialidad.Should().Be(doctores[0].Especialidad);
    }

    [Fact]
    public async Task ObtenerActivosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var doctores = new List<Doctor> { CrearDoctorEntidad() };

        _doctorRepository.ObtenerActivosAsync().Returns(doctores);

        // Act
        var resultado = (await _service.ObtenerActivosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].Estado.Should().Be(doctores[0].Estado);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var doctor = CrearDoctorEntidad();
        _doctorRepository.GetByIdAsync(doctor.Id).Returns(doctor);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(doctor.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(doctor.Id);
        resultado.CMP.Should().Be(doctor.CMP);
        resultado.CodigoDoctor.Should().Be(doctor.CodigoDoctor);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _doctorRepository.GetByIdAsync(id).Returns((Doctor?)null);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearAsync_SiCmpExiste_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var existente = CrearDoctorEntidad(cmp: dto.CMP);

        _doctorRepository.ObtenerPorCmpAsync(dto.CMP).Returns(existente);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe un doctor registrado con ese CMP.");

        await _doctorRepository.DidNotReceive().AddAsync(Arg.Any<Doctor>());
    }

    [Fact]
    public async Task CrearAsync_SiFechaFinEsMenor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new CrearDoctorDto
        {
            CMP = "12345",
            Nombres = "Luis",
            Apellidos = "Mamani",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = "doctor@correo.com",
            FechaInicioContrato = new DateTime(2026, 5, 10),
            FechaFinContrato = new DateTime(2026, 5, 1)
        };

        _doctorRepository.ObtenerPorCmpAsync(dto.CMP).Returns((Doctor?)null);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La fecha de fin de contrato no puede ser menor que la fecha de inicio.");
    }

    [Fact]
    public async Task CrearAsync_SiTodoEsValido_DebeCrearDoctorYGuardar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _doctorRepository.ObtenerPorCmpAsync(dto.CMP).Returns((Doctor?)null);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _doctorRepository.Received(1).AddAsync(Arg.Is<Doctor>(d =>
            d.CMP == dto.CMP &&
            d.Nombres == dto.Nombres &&
            d.Apellidos == dto.Apellidos &&
            d.Especialidad == dto.Especialidad &&
            d.Celular == dto.Celular &&
            d.Correo == dto.Correo &&
            d.UsuarioId == usuarioId &&
            !string.IsNullOrWhiteSpace(d.CodigoDoctor) &&
            d.CodigoDoctor.Contains("DOC-") &&
            d.CodigoDoctor.EndsWith(dto.CMP)));

        await _doctorRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ActualizarAsync_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = CrearEditarDtoValido();

        _doctorRepository.GetByIdAsync(id).Returns((Doctor?)null);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Doctor no encontrado.");
    }

    [Fact]
    public async Task ActualizarAsync_SiFechaFinEsMenor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doctor = CrearDoctorEntidad(id: id);

        var dto = new EditarDoctorDto
        {
            CMP = "54321",
            Nombres = "Carlos",
            Apellidos = "Quispe",
            Especialidad = "Obstetricia",
            Celular = "999888777",
            Correo = "editado@correo.com",
            FechaInicioContrato = new DateTime(2026, 6, 10),
            FechaFinContrato = new DateTime(2026, 6, 1),
            Estado = EstadoDoctor.Activo
        };

        _doctorRepository.GetByIdAsync(id).Returns(doctor);

        // Act
        Func<Task> act = async () => await _service.ActualizarAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("La fecha de fin de contrato no puede ser menor que la fecha de inicio.");
    }

    [Fact]
    public async Task ActualizarAsync_SiExiste_DebeActualizarYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doctor = CrearDoctorEntidad(id: id);
        var dto = CrearEditarDtoValido();

        _doctorRepository.GetByIdAsync(id).Returns(doctor);

        // Act
        await _service.ActualizarAsync(id, dto);

        // Assert
        doctor.CMP.Should().Be(dto.CMP);
        doctor.Nombres.Should().Be(dto.Nombres);
        doctor.Apellidos.Should().Be(dto.Apellidos);
        doctor.Especialidad.Should().Be(dto.Especialidad);
        doctor.Celular.Should().Be(dto.Celular);
        doctor.Correo.Should().Be(dto.Correo);
        doctor.Estado.Should().Be(dto.Estado);

        _doctorRepository.Received(1).Update(doctor);
        await _doctorRepository.Received(1).SaveChangesAsync();
    }

    private static CrearDoctorDto CrearDtoValido()
    {
        return new CrearDoctorDto
        {
            CMP = "12345",
            Nombres = "Luis",
            Apellidos = "Mamani",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = "doctor@correo.com",
            FechaInicioContrato = new DateTime(2026, 1, 10),
            FechaFinContrato = new DateTime(2026, 12, 31)
        };
    }

    private static EditarDoctorDto CrearEditarDtoValido()
    {
        return new EditarDoctorDto
        {
            CMP = "54321",
            Nombres = "Carlos",
            Apellidos = "Quispe",
            Especialidad = "Obstetricia",
            Celular = "999888777",
            Correo = "editado@correo.com",
            FechaInicioContrato = new DateTime(2026, 2, 1),
            FechaFinContrato = new DateTime(2026, 11, 30),
            Estado = EstadoDoctor.Inactivo
        };
    }

    private static Doctor CrearDoctorEntidad(Guid? id = null, string cmp = "12345")
    {
        return new Doctor
        {
            Id = id ?? Guid.NewGuid(),
            CodigoDoctor = $"DOC-ABCDE-{cmp}",
            CMP = cmp,
            Nombres = "Luis",
            Apellidos = "Mamani",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = "doctor@correo.com",
            FechaInicioContrato = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            FechaFinContrato = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            Estado = EstadoDoctor.Activo
        };
    }
    
    [Fact]
    public async Task CrearAsync_CuandoFechaFinEsNull_DebeCrearDoctor()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = new CrearDoctorDto
        {
            CMP = "99999",
            Nombres = "Luis",
            Apellidos = "Mamani",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = "doctor@correo.com",
            FechaInicioContrato = new DateTime(2026, 1, 10),
            FechaFinContrato = null   // 👈 rama HasValue false
        };

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _doctorRepository.ObtenerPorCmpAsync(dto.CMP).Returns((Doctor?)null);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();
        await _doctorRepository.Received(1).AddAsync(Arg.Is<Doctor>(d =>
            d.CMP == dto.CMP &&
            d.FechaFinContrato == null));
        await _doctorRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task ActualizarAsync_CuandoFechaFinEsNull_DebeActualizar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doctor = CrearDoctorEntidad(id: id);
        var dto = new EditarDoctorDto
        {
            CMP = "54321",
            Nombres = "Carlos",
            Apellidos = "Quispe",
            Especialidad = "Obstetricia",
            Celular = "999888777",
            Correo = "editado@correo.com",
            FechaInicioContrato = new DateTime(2026, 2, 1),
            FechaFinContrato = null,   // 👈 sin fecha fin
            Estado = EstadoDoctor.Activo
        };

        _doctorRepository.GetByIdAsync(id).Returns(doctor);

        // Act
        await _service.ActualizarAsync(id, dto);

        // Assert
        doctor.FechaFinContrato.Should().BeNull();
        _doctorRepository.Received(1).Update(doctor);
        await _doctorRepository.Received(1).SaveChangesAsync();
    }
}