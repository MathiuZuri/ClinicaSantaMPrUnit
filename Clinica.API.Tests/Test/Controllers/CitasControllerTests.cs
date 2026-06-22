using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Citas;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class CitasControllerTests
{
    private readonly ICitaService _citaService;
    private readonly CitasController _controller;

    public CitasControllerTests()
    {
        _citaService = Substitute.For<ICitaService>();
        _controller = new CitasController(_citaService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var citas = new List<CitaResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto()
        };

        _citaService.ObtenerTodasAsync().Returns(citas);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Citas obtenidas correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<CitaResponseDto>>();
        ((IEnumerable<CitaResponseDto>)response.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cita = CrearResponseDto(id: id);

        _citaService.ObtenerPorIdAsync(id).Returns(cita);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Cita obtenida correctamente.");
        response.Data.Should().BeOfType<CitaResponseDto>();
        ((CitaResponseDto)response.Data!).Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _citaService.ObtenerPorIdAsync(id).Returns((CitaResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetById(id);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Cita no encontrada.");
    }

    [Fact]
    public async Task GetByPaciente_DebeRetornarOkConApiResponse()
    {
        var pacienteId = Guid.NewGuid();
        var citas = new List<CitaResponseDto> { CrearResponseDto(pacienteId: pacienteId) };

        _citaService.ObtenerPorPacienteAsync(pacienteId).Returns(citas);

        var resultado = await _controller.GetByPaciente(pacienteId);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        // Ya no comprobamos ApiResponse<object>, solo verificamos las propiedades visibles
        var response = okResult.Value!;
    
        // Accedemos a las propiedades mediante dynamic o reflexión, o simplemente con un helper
        var exitoso = response.GetType().GetProperty("Exitoso")!.GetValue(response);
        exitoso.Should().Be(true);

        var mensaje = response.GetType().GetProperty("Mensaje")!.GetValue(response);
        mensaje.Should().Be("Citas del paciente obtenidas correctamente.");

        var data = response.GetType().GetProperty("Data")!.GetValue(response);
        data.Should().BeAssignableTo<IEnumerable<CitaResponseDto>>();
        (data as IEnumerable<CitaResponseDto>)!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByDoctor_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var citas = new List<CitaResponseDto> { CrearResponseDto(doctorId: doctorId) };

        _citaService.ObtenerPorDoctorAsync(doctorId).Returns(citas);

        // Act
        var resultado = await _controller.GetByDoctor(doctorId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Citas del doctor obtenidas correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<CitaResponseDto>>();
    }

    [Fact]
    public async Task Create_DebeRetornarCreatedAtActionConApiResponse()
    {
        // Arrange
        var dto = new CrearCitaDto
        {
            PacienteId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            HorarioDoctorId = Guid.NewGuid(),
            Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Primera consulta",
            Observaciones = "Obs"
        };

        var nuevoId = Guid.NewGuid();
        _citaService.CrearAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;

        createdResult.ActionName.Should().Be(nameof(CitasController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(nuevoId);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(201);
        response.Mensaje.Should().Be("Cita programada correctamente.");
    }

    [Fact]
    public async Task Reprogramar_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ReprogramarCitaDto
        {
            DoctorId = Guid.NewGuid(),
            HorarioDoctorId = Guid.NewGuid(),
            NuevaFecha = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            NuevaHoraInicio = new TimeOnly(10, 0),
            NuevaHoraFin = new TimeOnly(10, 30),
            MotivoReprogramacion = "Cambio solicitado"
        };

        _citaService.ReprogramarAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.Reprogramar(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Cita reprogramada correctamente.");
        response.Codigo.Should().Be(200);

        await _citaService.Received(1).ReprogramarAsync(id, dto);
    }

    [Fact]
    public async Task Cancelar_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CancelarCitaDto
        {
            MotivoCancelacion = "No podrá asistir"
        };

        _citaService.CancelarAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.Cancelar(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Cita cancelada correctamente.");
        response.Codigo.Should().Be(200);

        await _citaService.Received(1).CancelarAsync(id, dto);
    }

    private static CitaResponseDto CrearResponseDto(
        Guid? id = null,
        Guid? pacienteId = null,
        Guid? doctorId = null)
    {
        return new CitaResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            CodigoCita = "ABCDE-CIT-2026-12345678",
            PacienteId = pacienteId ?? Guid.NewGuid(),
            PacienteNombre = "Ana Quispe",
            DoctorId = doctorId ?? Guid.NewGuid(),
            DoctorNombre = "Luis Mamani",
            ServicioClinicoId = Guid.NewGuid(),
            ServicioNombre = "Consulta obstétrica",
            Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Motivo = "Control prenatal",
            Observaciones = "Obs",
            Estado = EstadoCita.Pendiente,
            FechaRegistro = DateTime.UtcNow
        };
    }
}