using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Horarios;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class HorariosControllerTests
{
    private readonly IHorarioDoctorService _horarioService;
    private readonly HorariosController _controller;

    public HorariosControllerTests()
    {
        _horarioService = Substitute.For<IHorarioDoctorService>();
        _controller = new HorariosController(_horarioService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var horarios = new List<HorarioDoctorResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto()
        };

        _horarioService.ObtenerTodosAsync().Returns(horarios);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Horarios obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<HorarioDoctorResponseDto>>();
        ((IEnumerable<HorarioDoctorResponseDto>)response.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDoctor_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var horarios = new List<HorarioDoctorResponseDto>
        {
            CrearResponseDto(doctorId: doctorId)
        };

        _horarioService.ObtenerPorDoctorAsync(doctorId).Returns(horarios);

        // Act
        var resultado = await _controller.GetByDoctor(doctorId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Horarios del doctor obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<HorarioDoctorResponseDto>>();
    }

    [Fact]
    public async Task Create_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var dto = new CrearHorarioDoctorDto
        {
            DoctorId = Guid.NewGuid(),
            DiaSemana = DayOfWeek.Monday,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(12, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.Today),
            FechaFinVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
        };

        var nuevoId = Guid.NewGuid();
        _horarioService.CrearAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.Create(dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Horario registrado correctamente.");

        await _horarioService.Received(1).CrearAsync(dto);
    }

    [Fact]
    public async Task Update_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new EditarHorarioDoctorDto
        {
            DiaSemana = DayOfWeek.Friday,
            HoraInicio = new TimeOnly(15, 0),
            HoraFin = new TimeOnly(19, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            FechaFinVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(45)),
            Activo = false
        };

        _horarioService.ActualizarAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.Update(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Horario actualizado correctamente.");

        await _horarioService.Received(1).ActualizarAsync(id, dto);
    }

    private static HorarioDoctorResponseDto CrearResponseDto(Guid? id = null, Guid? doctorId = null)
    {
        return new HorarioDoctorResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            DoctorId = doctorId ?? Guid.NewGuid(),
            DoctorNombre = "Luis Mamani",
            DiaSemana = DayOfWeek.Tuesday,
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.Today),
            FechaFinVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            Activo = true
        };
    }
    [Fact]
    public async Task GetMatrizSemanal_SinFecha_DeberiaRetornarOk()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var matriz = new MatrizSemanalDto { DoctorId = doctorId };
        _horarioService.ObtenerMatrizSemanalAsync(
            doctorId,
            Arg.Any<DateOnly>() // no podemos predecir la fecha de hoy, pero verificamos la llamada
        ).Returns(matriz);

        // Act
        var resultado = await _controller.GetMatrizSemanal(doctorId, null);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ApiResponse<object>>()
            .Which.Exitoso.Should().BeTrue();
    }
    [Fact]
    public async Task GetMatrizSemanal_ConFecha_DeberiaRetornarOk()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var fecha = "2026-05-12";
        var fechaEsperada = DateOnly.Parse(fecha);
        var matriz = new MatrizSemanalDto { DoctorId = doctorId };

        _horarioService.ObtenerMatrizSemanalAsync(doctorId, fechaEsperada)
            .Returns(matriz);

        // Act
        var resultado = await _controller.GetMatrizSemanal(doctorId, fecha);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ApiResponse<object>>()
            .Which.Exitoso.Should().BeTrue();
        await _horarioService.Received(1).ObtenerMatrizSemanalAsync(doctorId, fechaEsperada);
    }
}