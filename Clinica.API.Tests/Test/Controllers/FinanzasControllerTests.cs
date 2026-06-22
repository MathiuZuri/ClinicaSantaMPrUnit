using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.Domain.DTOs.Finanzas;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class FinanzasControllerTests
{
    private readonly IFinanzasService _finanzasService;
    private readonly FinanzasController _controller;

    public FinanzasControllerTests()
    {
        _finanzasService = Substitute.For<IFinanzasService>();
        _controller = new FinanzasController(_finanzasService);
    }

    [Fact]
    public async Task ObtenerResumenDiario_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var fecha = new DateOnly(2026, 1, 10);
        var dto = new ResumenDiarioFinanzasDto { Fecha = fecha };

        _finanzasService.ObtenerResumenDiarioAsync(fecha).Returns(dto);

        // Act
        var resultado = await _controller.ObtenerResumenDiario(fecha);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Resumen diario de finanzas obtenido correctamente.");
    }

    [Fact]
    public async Task ObtenerResumenMensual_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerResumenMensualAsync(2026, 3)
            .Returns(new ResumenMensualFinanzasDto { Anio = 2026, Mes = 3 });

        // Act
        var resultado = await _controller.ObtenerResumenMensual(2026, 3);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Resumen mensual de finanzas obtenido correctamente.");
    }

    [Fact]
    public async Task ObtenerResumenAnual_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerResumenAnualAsync(2026)
            .Returns(new ResumenAnualFinanzasDto { Anio = 2026 });

        // Act
        var resultado = await _controller.ObtenerResumenAnual(2026);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Resumen anual de finanzas obtenido correctamente.");
    }

    [Fact]
    public async Task ObtenerPagosPendientes_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerPagosPendientesAsync()
            .Returns(new List<PagoFinanzasDto>());

        // Act
        var resultado = await _controller.ObtenerPagosPendientes();

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Pagos pendientes obtenidos correctamente.");
    }

    [Fact]
    public async Task ObtenerPagosPagados_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerPagosPagadosAsync()
            .Returns(new List<PagoFinanzasDto>());

        // Act
        var resultado = await _controller.ObtenerPagosPagados();

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Pagos pagados obtenidos correctamente.");
    }

    [Fact]
    public async Task ObtenerPagosParciales_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerPagosParcialesAsync()
            .Returns(new List<PagoFinanzasDto>());

        // Act
        var resultado = await _controller.ObtenerPagosParciales();

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Pagos parciales obtenidos correctamente.");
    }

    [Fact]
    public async Task ObtenerPagoPorCodigo_SiExiste_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerPagoPorCodigoAsync("CODIGO")
            .Returns(new PagoFinanzasDto { CodigoPago = "CODIGO" });

        // Act
        var resultado = await _controller.ObtenerPagoPorCodigo("CODIGO");

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Pago obtenido correctamente.");
    }

    [Fact]
    public async Task ObtenerPagoPorCodigo_SiNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        _finanzasService.ObtenerPagoPorCodigoAsync("CODIGO").Returns((PagoFinanzasDto?)null);

        // Act
        Func<Task> act = async () => await _controller.ObtenerPagoPorCodigo("CODIGO");

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Pago no encontrado.");
    }

    [Fact]
    public async Task ObtenerEstadoCuentaPaciente_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        _finanzasService.ObtenerEstadoCuentaPacienteAsync(pacienteId)
            .Returns(new EstadoCuentaPacienteDto { PacienteId = pacienteId });

        // Act
        var resultado = await _controller.ObtenerEstadoCuentaPaciente(pacienteId);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Estado de cuenta del paciente obtenido correctamente.");
    }

    [Fact]
    public async Task ObtenerDeudasReales_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerDeudasRealesAsync()
            .Returns(new List<EstadoPagoAtencionDto>());

        // Act
        var resultado = await _controller.ObtenerDeudasReales();

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Deudas reales obtenidas correctamente.");
    }

    [Fact]
    public async Task ObtenerDeudasRealesPaciente_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        _finanzasService.ObtenerDeudasRealesPacienteAsync(pacienteId)
            .Returns(new List<EstadoPagoAtencionDto>());

        // Act
        var resultado = await _controller.ObtenerDeudasRealesPaciente(pacienteId);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Deudas reales del paciente obtenidas correctamente.");
    }

    [Fact]
    public async Task ObtenerEstadoPagoAtencion_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var atencionId = Guid.NewGuid();
        _finanzasService.ObtenerEstadoPagoAtencionAsync(atencionId)
            .Returns(new EstadoPagoAtencionDto { AtencionId = atencionId });

        // Act
        var resultado = await _controller.ObtenerEstadoPagoAtencion(atencionId);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Estado de pago de la atención obtenido correctamente.");
    }

    [Fact]
    public async Task ObtenerLibroDiario_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var fecha = new DateOnly(2026, 3, 10);
        _finanzasService.ObtenerLibroDiarioAsync(fecha)
            .Returns(new List<PagoFinanzasDto>());

        // Act
        var resultado = await _controller.ObtenerLibroDiario(fecha);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Libro diario obtenido correctamente.");
    }

    [Fact]
    public async Task ObtenerResumenFinancieroMensualCompleto_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerResumenFinancieroMensualCompletoAsync(2026, 3)
            .Returns(new ResumenFinancieroMensualCompletoDto { Anio = 2026, Mes = 3 });

        // Act
        var resultado = await _controller.ObtenerResumenFinancieroMensualCompleto(2026, 3);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Resumen financiero mensual completo obtenido correctamente.");
    }

    [Fact]
    public async Task RegistrarAjusteFinanciero_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 10m,
            Motivo = "Motivo válido"
        };

        var nuevoId = Guid.NewGuid();
        _finanzasService.RegistrarAjusteFinancieroAsync(dto).Returns(nuevoId);

        // Act
        var resultado = await _controller.RegistrarAjusteFinanciero(dto);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Ajuste financiero registrado correctamente.");

        await _finanzasService.Received(1).RegistrarAjusteFinancieroAsync(dto);
    }

    [Fact]
    public async Task ObtenerAjustesFinancieros_DebeRetornarOkConApiResponse()
    {
        // Arrange
        _finanzasService.ObtenerAjustesFinancierosAsync()
            .Returns(new List<AjusteFinancieroDto>());

        // Act
        var resultado = await _controller.ObtenerAjustesFinancieros();

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Ajustes financieros obtenidos correctamente.");
    }

    [Fact]
    public async Task ObtenerAjustesPorAtencion_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var atencionId = Guid.NewGuid();
        _finanzasService.ObtenerAjustesPorAtencionAsync(atencionId)
            .Returns(new List<AjusteFinancieroDto>());

        // Act
        var resultado = await _controller.ObtenerAjustesPorAtencion(atencionId);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Ajustes financieros de la atención obtenidos correctamente.");
    }

    [Fact]
    public async Task ObtenerAjustesPorPago_DebeRetornarOkConApiResponse()
    {
        // Arrange
        var pagoId = Guid.NewGuid();
        _finanzasService.ObtenerAjustesPorPagoAsync(pagoId)
            .Returns(new List<AjusteFinancieroDto>());

        // Act
        var resultado = await _controller.ObtenerAjustesPorPago(pagoId);

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Mensaje.Should().Be("Ajustes financieros del pago obtenidos correctamente.");
    }
    
    [Fact]
    public void ObtenerTasaIGV_DebeRetornarTasaCorrecta()
    {
        // El método es sincrónico, no lleva await
        var result = _controller.ObtenerTasaIgv();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        apiResponse.Exitoso.Should().BeTrue();
        apiResponse.Mensaje.Should().Be("Tasa de IGV actual.");
        apiResponse.Data.Should().Be(18m);
    }
}