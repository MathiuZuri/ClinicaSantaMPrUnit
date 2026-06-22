using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Pagos;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class PagosControllerTests
{
    private readonly IPagoService _pagoService;
    private readonly PagosController _controller;

    public PagosControllerTests()
    {
        _pagoService = Substitute.For<IPagoService>();
        _controller = new PagosController(_pagoService);
    }

    [Fact]
    public async Task GetByPaciente_DebeRetornarOkConApiResponse()
    {
        var pacienteId = Guid.NewGuid();
        var pagos = new List<PagoResponseDto> { CrearResponseDto(pacienteId: pacienteId) };

        _pagoService.ObtenerPorPacienteAsync(pacienteId).Returns(pagos);

        var resultado = await _controller.GetByPaciente(pacienteId);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value!;

        var exitoso = response.GetType().GetProperty("Exitoso")!.GetValue(response);
        exitoso.Should().Be(true);

        var mensaje = response.GetType().GetProperty("Mensaje")!.GetValue(response);
        mensaje.Should().Be("Pagos del paciente obtenidos correctamente.");

        var data = response.GetType().GetProperty("Data")!.GetValue(response);
        data.Should().BeAssignableTo<IEnumerable<PagoResponseDto>>();
        (data as IEnumerable<PagoResponseDto>)!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByCita_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var citaId = Guid.NewGuid();
        var pagos = new List<PagoResponseDto> { CrearResponseDto(citaId: citaId) };

        _pagoService.ObtenerPorCitaAsync(citaId).Returns(pagos);

        // Act
        var resultado = await _controller.GetByCita(citaId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Pagos de la cita obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<PagoResponseDto>>();
    }

    [Fact]
    public async Task GetByAtencion_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var atencionId = Guid.NewGuid();
        var pagos = new List<PagoResponseDto> { CrearResponseDto(atencionId: atencionId) };

        _pagoService.ObtenerPorAtencionAsync(atencionId).Returns(pagos);

        // Act
        var resultado = await _controller.GetByAtencion(atencionId);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Pagos de la atención obtenidos correctamente.");
        response.Data.Should().BeAssignableTo<IEnumerable<PagoResponseDto>>();
    }

    [Fact]
    public async Task Registrar_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var dto = new RegistrarPagoDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            CitaId = Guid.NewGuid(),
            AtencionId = null,
            MontoTotal = 100m,
            MontoPagado = 60m,
            MontoAdelanto = 20m,
            MetodoPago = MetodoPago.Efectivo,
            Observacion = "Pago inicial"
        };

        var nuevoId = Guid.NewGuid();
        _pagoService.RegistrarAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.Registrar(dto);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Pago registrado correctamente.");

        await _pagoService.Received(1).RegistrarAsync(dto);
    }

    private static PagoResponseDto CrearResponseDto(
        Guid? pacienteId = null,
        Guid? citaId = null,
        Guid? atencionId = null)
    {
        return new PagoResponseDto
        {
            Id = Guid.NewGuid(),
            CodigoPago = "ABCDE-PAG-2026-12345678",
            PacienteId = pacienteId ?? Guid.NewGuid(),
            PacienteNombre = "Ana Quispe",
            ServicioClinicoId = Guid.NewGuid(),
            ServicioNombre = "Atención general",
            CitaId = citaId,
            AtencionId = atencionId,
            MontoTotal = 100m,
            MontoPagado = 60m,
            SaldoPendiente = 40m,
            MontoAdelanto = 20m,
            MetodoPago = MetodoPago.Efectivo,
            Estado = EstadoPago.Parcial,
            Observacion = "Pago inicial",
            FechaPago = DateTime.UtcNow
        };
    }
    
    [Fact]
    public async Task CambiarEstado_DebeRetornarOkConApiResponse()
    {
        var id = Guid.NewGuid();
        var dto = new CambiarEstadoPagoDto { Estado = EstadoPago.Pagado };

        _pagoService.CambiarEstadoAsync(id, dto).Returns(Task.CompletedTask);

        var resultado = await _controller.CambiarEstado(id, dto);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeTrue();
        response.Codigo.Should().Be(200);
        response.Mensaje.Should().Be("Estado del pago actualizado correctamente.");

        await _pagoService.Received(1).CambiarEstadoAsync(id, dto);
    }
}