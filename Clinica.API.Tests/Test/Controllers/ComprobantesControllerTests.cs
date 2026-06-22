using Clinica.API.Controllers;
using Clinica.Domain.DTOs.Comprobantes;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class ComprobantesControllerTests
{
    private readonly IComprobanteService _service;
    private readonly ComprobantesController _controller;

    public ComprobantesControllerTests()
    {
        _service = Substitute.For<IComprobanteService>();
        _controller = new ComprobantesController(_service);
    }

    // Previews
    [Fact]
    public async Task PreviewBoletaPago_RetornaOk()
    {
        var pagoId = Guid.NewGuid();
        var dto = new ComprobantePagoPreviewDto();
        _service.PreviewBoletaPagoAsync(pagoId).Returns(dto);
        var result = await _controller.PreviewBoletaPago(pagoId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PreviewConstanciaCita_RetornaOk()
    {
        var citaId = Guid.NewGuid();
        var dto = new ComprobanteCitaPreviewDto();
        _service.PreviewConstanciaCitaAsync(citaId).Returns(dto);
        var result = await _controller.PreviewConstanciaCita(citaId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PreviewResumenAtencion_RetornaOk()
    {
        var id = Guid.NewGuid();
        _service.PreviewResumenAtencionAsync(id).Returns(new ComprobanteAtencionPreviewDto());
        var result = await _controller.PreviewResumenAtencion(id);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PreviewEstadoCuenta_RetornaOk()
    {
        var id = Guid.NewGuid();
        _service.PreviewEstadoCuentaPacienteAsync(id).Returns(new ComprobanteEstadoCuentaPreviewDto());
        var result = await _controller.PreviewEstadoCuentaPaciente(id);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // Emisión
    [Fact]
    public async Task EmitirBoletaPago_RetornaOkConId()
    {
        var dto = new EmitirComprobantePagoDto { PagoId = Guid.NewGuid() };
        var id = Guid.NewGuid();
        _service.EmitirBoletaPagoAsync(dto).Returns(id);
        var result = await _controller.EmitirBoletaPago(dto);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EmitirConstanciaCita_RetornaOkConId()
    {
        var dto = new EmitirComprobanteCitaDto { CitaId = Guid.NewGuid() };
        _service.EmitirConstanciaCitaAsync(dto).Returns(Guid.NewGuid());
        var result = await _controller.EmitirConstanciaCita(dto);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EmitirResumenAtencion_RetornaOk()
    {
        var dto = new EmitirComprobanteAtencionDto { AtencionId = Guid.NewGuid() };
        _service.EmitirResumenAtencionAsync(dto).Returns(Guid.NewGuid());
        var result = await _controller.EmitirResumenAtencion(dto);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EmitirEstadoCuenta_RetornaOk()
    {
        var dto = new EmitirComprobanteEstadoCuentaDto { PacienteId = Guid.NewGuid() };
        _service.EmitirEstadoCuentaPacienteAsync(dto).Returns(Guid.NewGuid());
        var result = await _controller.EmitirEstadoCuenta(dto);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // PDF
    [Fact]
    public async Task GenerarPdfBoletaPago_RetornaFile()
    {
        var id = Guid.NewGuid();
        var doc = new DocumentoGeneradoDto { Archivo = new byte[] { 1, 2, 3 }, ContentType = "application/pdf", NombreArchivo = "test.pdf" };
        _service.GenerarPdfBoletaPagoAsync(id).Returns(doc);
        var result = await _controller.GenerarPdfBoletaPago(id);
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task GenerarPdfConstanciaCita_RetornaFile()
    {
        var id = Guid.NewGuid();
        var doc = new DocumentoGeneradoDto { Archivo = new byte[] { 1 }, ContentType = "application/pdf", NombreArchivo = "x.pdf" };
        _service.GenerarPdfConstanciaCitaAsync(id).Returns(doc);
        var result = await _controller.GenerarPdfConstanciaCita(id);
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task GenerarPdfResumenAtencion_RetornaFile()
    {
        var id = Guid.NewGuid();
        _service.GenerarPdfResumenAtencionAsync(id).Returns(new DocumentoGeneradoDto { Archivo = new byte[] { 1 } });
        var result = await _controller.GenerarPdfResumenAtencion(id);
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task GenerarPdfEstadoCuenta_RetornaFile()
    {
        var id = Guid.NewGuid();
        _service.GenerarPdfEstadoCuentaPacienteAsync(id).Returns(new DocumentoGeneradoDto { Archivo = new byte[] { 1 } });
        var result = await _controller.GenerarPdfEstadoCuenta(id);
        result.Should().BeOfType<FileContentResult>();
    }

    // Consultas
    [Fact]
    public async Task ObtenerPorId_RetornaOk()
    {
        var id = Guid.NewGuid();
        _service.ObtenerPorIdAsync(id).Returns(new ComprobanteDto());
        var result = await _controller.ObtenerPorId(id);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObtenerPorPaciente_RetornaOk()
    {
        var id = Guid.NewGuid();
        _service.ObtenerPorPacienteAsync(id).Returns(new List<ComprobanteDto>());
        var result = await _controller.ObtenerPorPaciente(id);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObtenerPorPago_RetornaOk()
    {
        var id = Guid.NewGuid();
        _service.ObtenerPorPagoAsync(id).Returns(new List<ComprobanteDto>());
        var result = await _controller.ObtenerPorPago(id);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObtenerPorAtencion_RetornaOk()
    {
        var id = Guid.NewGuid();
        _service.ObtenerPorAtencionAsync(id).Returns(new List<ComprobanteDto>());
        var result = await _controller.ObtenerPorAtencion(id);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // Anulación
    [Fact]
    public async Task AnularComprobante_RetornaOk()
    {
        var id = Guid.NewGuid();
        _service.AnularComprobanteAsync(id, "motivo").Returns(Task.CompletedTask);
        var result = await _controller.AnularComprobante(id, new ComprobantesController.AnularComprobanteRequest { Motivo = "motivo" });
        result.Result.Should().BeOfType<OkObjectResult>();
    }
}