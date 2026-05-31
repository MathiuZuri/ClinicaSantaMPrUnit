using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Pacientes;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class PacienteServiceTests
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _historialDetalleRepository;
    private readonly IUsuarioActualService _usuarioActualService;

    private readonly IPacienteService _service;

    public PacienteServiceTests()
    {
        _pacienteRepository = Substitute.For<IPacienteRepository>();
        _historialRepository = Substitute.For<IHistorialClinicoRepository>();
        _historialDetalleRepository = Substitute.For<IHistorialDetalleRepository>();
        _usuarioActualService = Substitute.For<IUsuarioActualService>();

        _service = new PacienteService(
            _pacienteRepository,
            _historialRepository,
            _historialDetalleRepository,
            _usuarioActualService);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var pacientes = new List<Paciente>
        {
            CrearPacienteEntidad(),
            CrearPacienteEntidad()
        };

        _pacienteRepository.GetAllAsync().Returns(pacientes);

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(pacientes[0].Id);
        resultado[0].CodigoPaciente.Should().Be(pacientes[0].CodigoPaciente);
        resultado[0].DNI.Should().Be(pacientes[0].DNI);
        resultado[0].Nombres.Should().Be(pacientes[0].Nombres);
        resultado[0].Apellidos.Should().Be(pacientes[0].Apellidos);
        resultado[0].CodigoHistorial.Should().Be(pacientes[0].HistorialClinico?.CodigoHistorial);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var paciente = CrearPacienteEntidad();
        _pacienteRepository.ObtenerConHistorialAsync(paciente.Id).Returns(paciente);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(paciente.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(paciente.Id);
        resultado.CodigoPaciente.Should().Be(paciente.CodigoPaciente);
        resultado.DNI.Should().Be(paciente.DNI);
        resultado.CodigoHistorial.Should().Be(paciente.HistorialClinico?.CodigoHistorial);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _pacienteRepository.ObtenerConHistorialAsync(id).Returns((Paciente?)null);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPorDniAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var paciente = CrearPacienteEntidad();
        _pacienteRepository.ObtenerPorDniAsync(paciente.DNI).Returns(paciente);

        // Act
        var resultado = await _service.ObtenerPorDniAsync(paciente.DNI);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.DNI.Should().Be(paciente.DNI);
        resultado.CodigoPaciente.Should().Be(paciente.CodigoPaciente);
    }

    [Fact]
    public async Task ObtenerPorDniAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        const string dni = "12345678";
        _pacienteRepository.ObtenerPorDniAsync(dni).Returns((Paciente?)null);

        // Act
        var resultado = await _service.ObtenerPorDniAsync(dni);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearAsync_SiYaExisteDni_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = CrearDtoValido();
        var existente = CrearPacienteEntidad(dni: dto.DNI);

        _usuarioActualService.ObtenerUsuarioId().Returns(Guid.NewGuid());
        _pacienteRepository.ObtenerPorDniAsync(dto.DNI).Returns(existente);

        // Act
        Func<Task> act = async () => await _service.CrearAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe un paciente registrado con ese DNI.");

        await _pacienteRepository.DidNotReceive().AddAsync(Arg.Any<Paciente>());
        await _historialRepository.DidNotReceive().AddAsync(Arg.Any<HistorialClinico>());
        await _historialDetalleRepository.DidNotReceive().AddAsync(Arg.Any<HistorialDetalle>());
    }

    [Fact]
    public async Task CrearAsync_SiDniNoExiste_DebeCrearPacienteHistorialYDetalle()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = CrearDtoValido();

        _usuarioActualService.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepository.ObtenerPorDniAsync(dto.DNI).Returns((Paciente?)null);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeEmpty();

        await _pacienteRepository.Received(1).AddAsync(Arg.Is<Paciente>(p =>
            p.DNI == dto.DNI &&
            p.Nombres == dto.Nombres &&
            p.Apellidos == dto.Apellidos &&
            p.Sexo == dto.Sexo &&
            p.Celular == dto.Celular &&
            p.Correo == dto.Correo &&
            p.Direccion == dto.Direccion &&
            p.UsuarioId == usuarioId &&
            !string.IsNullOrWhiteSpace(p.CodigoPaciente) &&
            p.CodigoPaciente.Contains("-2026-") || p.CodigoPaciente.Contains($"PCT-{DateTime.UtcNow:yyyy}-")
        ));

        await _historialRepository.Received(1).AddAsync(Arg.Is<HistorialClinico>(h =>
            h.PacienteId != Guid.Empty &&
            !string.IsNullOrWhiteSpace(h.CodigoHistorial)
        ));

        await _historialDetalleRepository.Received(1).AddAsync(Arg.Is<HistorialDetalle>(d =>
            d.TipoMovimiento == TipoMovimientoHistorial.AperturaHistorial &&
            d.Titulo == "Apertura de historial clínico" &&
            d.Descripcion == "Se registró al paciente y se aperturó su historial clínico." &&
            d.UsuarioId == usuarioId &&
            !string.IsNullOrWhiteSpace(d.CodigoDetalle)
        ));

        await _pacienteRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ActualizarContactoAsync_SiPacienteNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ActualizarContactoPacienteDto
        {
            Celular = "987654321",
            Correo = "nuevo@correo.com",
            Direccion = "Nueva dirección"
        };

        _pacienteRepository.GetByIdAsync(id).Returns((Paciente?)null);

        // Act
        Func<Task> act = async () => await _service.ActualizarContactoAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task ActualizarContactoAsync_SiPacienteExiste_DebeActualizarYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paciente = CrearPacienteEntidad(id: id);

        var dto = new ActualizarContactoPacienteDto
        {
            Celular = "999888777",
            Correo = "actualizado@correo.com",
            Direccion = "Dirección actualizada"
        };

        _pacienteRepository.GetByIdAsync(id).Returns(paciente);

        // Act
        await _service.ActualizarContactoAsync(id, dto);

        // Assert
        paciente.Celular.Should().Be(dto.Celular);
        paciente.Correo.Should().Be(dto.Correo);
        paciente.Direccion.Should().Be(dto.Direccion);

        _pacienteRepository.Received(1).Update(paciente);
        await _pacienteRepository.Received(1).SaveChangesAsync();
    }

    private static CrearPacienteDto CrearDtoValido()
    {
        return new CrearPacienteDto
        {
            DNI = "12345678",
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(2000, 5, 10),
            Sexo = "F",
            Celular = "987654321",
            Correo = "ana@correo.com",
            Direccion = "Juliaca"
        };
    }

    private static Paciente CrearPacienteEntidad(Guid? id = null, string dni = "12345678")
    {
        return new Paciente
        {
            Id = id ?? Guid.NewGuid(),
            CodigoPaciente = "PCT-2026-ABCDE-12345678",
            DNI = dni,
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(2000, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Sexo = "F",
            Celular = "987654321",
            Correo = "ana@correo.com",
            Direccion = "Juliaca",
            Estado = EstadoPaciente.Activo,
            FechaRegistro = DateTime.UtcNow,
            HistorialClinico = new HistorialClinico
            {
                Id = Guid.NewGuid(),
                CodigoHistorial = "ABCDE-2026-12345678"
            }
        };
    }
    
    [Fact]
    public async Task CambiarEstadoAsync_SiPacienteExiste_DebeActualizarEstadoYGuardar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paciente = CrearPacienteEntidad(id: id);
        var dto = new CambiarEstadoPacienteDto { Estado = EstadoPaciente.Inactivo };

        _pacienteRepository.GetByIdAsync(id).Returns(paciente);

        // Act
        await _service.CambiarEstadoAsync(id, dto);

        // Assert
        paciente.Estado.Should().Be(EstadoPaciente.Inactivo);
        _pacienteRepository.Received(1).Update(paciente);
        await _pacienteRepository.Received(1).SaveChangesAsync();
    }
    [Fact]
    public async Task CambiarEstadoAsync_SiPacienteNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CambiarEstadoPacienteDto { Estado = EstadoPaciente.Inactivo };

        _pacienteRepository.GetByIdAsync(id).Returns((Paciente?)null);

        // Act
        Func<Task> act = async () => await _service.CambiarEstadoAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }
    [Fact]
    public async Task CambiarEstadoAsync_SiPacienteEliminado_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paciente = CrearPacienteEntidad(id: id);
        paciente.Estado = EstadoPaciente.Eliminado; // forzar estado
        var dto = new CambiarEstadoPacienteDto { Estado = EstadoPaciente.Inactivo };

        _pacienteRepository.GetByIdAsync(id).Returns(paciente);

        // Act
        Func<Task> act = async () => await _service.CambiarEstadoAsync(id, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede cambiar el estado de un paciente eliminado.");

        _pacienteRepository.DidNotReceive().Update(Arg.Any<Paciente>());
        await _pacienteRepository.DidNotReceive().SaveChangesAsync();
    }
    [Fact]
    public async Task ObtenerTodosAsync_CuandoHistorialClinicoEsNull_DebeRetornarCodigoHistorialNull()
    {
        // Arrange
        var paciente = CrearPacienteEntidad();
        paciente.HistorialClinico = null;

        _pacienteRepository.GetAllAsync().Returns(new List<Paciente> { paciente });

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].CodigoHistorial.Should().BeNull();
    }
    [Fact]
    public async Task ObtenerPorIdAsync_CuandoHistorialClinicoEsNull_DebeRetornarCodigoHistorialNull()
    {
        // Arrange
        var paciente = CrearPacienteEntidad();
        paciente.HistorialClinico = null;
        _pacienteRepository.ObtenerConHistorialAsync(paciente.Id).Returns(paciente);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(paciente.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.CodigoHistorial.Should().BeNull();
    }
    [Fact]
    public async Task ObtenerPorDniAsync_CuandoHistorialClinicoEsNull_DebeRetornarCodigoHistorialNull()
    {
        // Arrange
        var paciente = CrearPacienteEntidad();
        paciente.HistorialClinico = null;
        _pacienteRepository.ObtenerPorDniAsync(paciente.DNI).Returns(paciente);

        // Act
        var resultado = await _service.ObtenerPorDniAsync(paciente.DNI);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.CodigoHistorial.Should().BeNull();
    }
    
}