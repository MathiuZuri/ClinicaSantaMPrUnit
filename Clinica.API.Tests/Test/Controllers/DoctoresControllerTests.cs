using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class DoctoresControllerTests
{
    private readonly IDoctorService _doctorService;
    private readonly DoctoresController _controller;

    public DoctoresControllerTests()
    {
        _doctorService = Substitute.For<IDoctorService>();
        _controller = new DoctoresController(_doctorService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var doctores = new List<DoctorResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto()
        };

        _doctorService.ObtenerTodosAsync().Returns(doctores);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Doctores obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<DoctorResponseDto>>();
        ((IEnumerable<DoctorResponseDto>)response.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActivos_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var doctores = new List<DoctorResponseDto> { CrearResponseDto() };

        _doctorService.ObtenerActivosAsync().Returns(doctores);

        // Act
        var resultado = await _controller.GetActivos();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Doctores activos obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<DoctorResponseDto>>();
    }

    [Fact]
    public async Task GetById_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doctor = CrearResponseDto(id: id);

        _doctorService.ObtenerPorIdAsync(id).Returns(doctor);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Doctor obtenido correctamente.");
        response.Data.Should().BeOfType<DoctorResponseDto>();
        ((DoctorResponseDto)response.Data!).Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _doctorService.ObtenerPorIdAsync(id).Returns((DoctorResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetById(id);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Doctor no encontrado.");
    }

    [Fact]
    public async Task Create_DebeRetornarCreatedAtActionConApiResponse()
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
            FechaInicioContrato = new DateTime(2026, 1, 10),
            FechaFinContrato = new DateTime(2026, 12, 31)
        };

        var nuevoId = Guid.NewGuid();
        _doctorService.CrearAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;

        createdResult.ActionName.Should().Be(nameof(DoctoresController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(nuevoId);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(201);
        response.Mensaje.Should().Be("Doctor registrado correctamente.");
    }

    [Fact]
    public async Task Update_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new EditarDoctorDto
        {
            CMP = "54321",
            Nombres = "Carlos",
            Apellidos = "Quispe",
            Especialidad = "Obstetricia",
            Celular = "999888777",
            Correo = "editado@correo.com",
            FechaInicioContrato = new DateTime(2026, 2, 1),
            FechaFinContrato = new DateTime(2026, 11, 30),
            Estado = EstadoDoctor.Activo
        };

        _doctorService.ActualizarAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.Update(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Doctor actualizado correctamente.");

        await _doctorService.Received(1).ActualizarAsync(id, dto);
    }

    private static DoctorResponseDto CrearResponseDto(Guid? id = null)
    {
        return new DoctorResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            CodigoDoctor = "DOC-ABCDE-12345",
            CMP = "12345",
            Nombres = "Luis",
            Apellidos = "Mamani",
            Especialidad = "Ginecología",
            Celular = "987654321",
            Correo = "doctor@correo.com",
            FechaInicioContrato = new DateTime(2026, 1, 10),
            FechaFinContrato = new DateTime(2026, 12, 31),
            Estado = EstadoDoctor.Activo
        };
    }
}