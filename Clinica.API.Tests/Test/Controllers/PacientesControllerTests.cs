using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Pacientes;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class PacientesControllerTests
{
    private readonly IPacienteService _pacienteService;
    private readonly PacientesController _controller;

    public PacientesControllerTests()
    {
        _pacienteService = Substitute.For<IPacienteService>();
        _controller = new PacientesController(_pacienteService);
    }

    [Fact]
    public async Task GetAll_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var pacientes = new List<PacienteResponseDto>
        {
            CrearResponseDto(),
            CrearResponseDto()
        };

        _pacienteService.ObtenerTodosAsync().Returns(pacientes);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Pacientes obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<PacienteResponseDto>>();
        ((IEnumerable<PacienteResponseDto>)response.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paciente = CrearResponseDto(id: id);

        _pacienteService.ObtenerPorIdAsync(id).Returns(paciente);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Paciente obtenido correctamente.");
        response.Data.Should().BeOfType<PacienteResponseDto>();
        ((PacienteResponseDto)response.Data!).Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _pacienteService.ObtenerPorIdAsync(id).Returns((PacienteResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetById(id);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task GetByDni_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        const string dni = "12345678";
        var paciente = CrearResponseDto(dni: dni);

        _pacienteService.ObtenerPorDniAsync(dni).Returns(paciente);

        // Act
        var resultado = await _controller.GetByDni(dni);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Paciente obtenido correctamente.");
        response.Data.Should().BeOfType<PacienteResponseDto>();
        ((PacienteResponseDto)response.Data!).DNI.Should().Be(dni);
    }

    [Fact]
    public async Task GetByDni_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        const string dni = "12345678";
        _pacienteService.ObtenerPorDniAsync(dni).Returns((PacienteResponseDto?)null);

        // Act
        Func<Task> act = async () => await _controller.GetByDni(dni);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task Create_DebeRetornarCreatedAtActionConApiResponse()
    {
        // Arrange
        var dto = new CrearPacienteDto
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

        var nuevoId = Guid.NewGuid();
        _pacienteService.CrearAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;

        createdResult.ActionName.Should().Be(nameof(PacientesController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(nuevoId);

        var response = createdResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(201);
        response.Mensaje.Should().Be("Paciente registrado correctamente.");
    }

    [Fact]
    public async Task UpdateContact_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ActualizarContactoPacienteDto
        {
            Celular = "999888777",
            Correo = "actualizado@correo.com",
            Direccion = "Nueva dirección"
        };

        _pacienteService.ActualizarContactoAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.UpdateContact(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Contacto del paciente actualizado correctamente.");
        response.Codigo.Should().Be(200);

        await _pacienteService.Received(1).ActualizarContactoAsync(id, dto);
    }

    private static PacienteResponseDto CrearResponseDto(Guid? id = null, string dni = "12345678")
    {
        return new PacienteResponseDto
        {
            Id = id ?? Guid.NewGuid(),
            CodigoPaciente = "PCT-2026-ABCDE-12345678",
            DNI = dni,
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = new DateTime(2000, 5, 10),
            Sexo = "F",
            Celular = "987654321",
            Correo = "ana@correo.com",
            Direccion = "Juliaca",
            Estado = EstadoPaciente.Activo,
            FechaRegistro = DateTime.UtcNow,
            CodigoHistorial = "ABCDE-2026-12345678"
        };
    }
    
    [Fact]
    public async Task CambiarEstado_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CambiarEstadoPacienteDto { Estado = EstadoPaciente.Inactivo };

        _pacienteService.CambiarEstadoAsync(id, dto).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.CambiarEstado(id, dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Mensaje.Should().Be("Estado del paciente actualizado correctamente.");
        response.Codigo.Should().Be(200);

        await _pacienteService.Received(1).CambiarEstadoAsync(id, dto);
    }
}