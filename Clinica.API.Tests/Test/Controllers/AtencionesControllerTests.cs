using Clinica.API.Controllers;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Atenciones;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class AtencionesControllerTests
{
    private readonly IAtencionService _atencionService;
    private readonly AtencionesController _controller;

    public AtencionesControllerTests()
    {
        _atencionService = Substitute.For<IAtencionService>();
        _controller = new AtencionesController(_atencionService);
    }

    [Fact]
    public async Task ObtenerTodas_DebeRetornarOkConLista()
    {
        var lista = new List<AtencionResponseDto> { new() { Id = Guid.NewGuid() } };
        _atencionService.ObtenerTodasAsync().Returns(lista);

        var resultado = await _controller.ObtenerTodas();

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var resp = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        resp.Exitoso.Should().BeTrue();
        resp.Mensaje.Should().Be("Atenciones obtenidas correctamente.");
    }

    [Fact]
    public async Task ObtenerPorPaciente_DebeRetornarOkConLista()
    {
        var pacienteId = Guid.NewGuid();
        var lista = new List<AtencionResponseDto> { new() { Id = Guid.NewGuid() } };
        _atencionService.ObtenerPorPacienteAsync(pacienteId).Returns(lista);

        var resultado = await _controller.ObtenerPorPaciente(pacienteId);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObtenerPorId_Existente_RetornaOk()
    {
        var id = Guid.NewGuid();
        var atencion = new AtencionResponseDto { Id = id };
        _atencionService.ObtenerPorIdAsync(id).Returns(atencion);

        var resultado = await _controller.ObtenerPorId(id);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_RetornaNotFound()
    {
        var id = Guid.NewGuid();
        _atencionService.ObtenerPorIdAsync(id).Returns((AtencionResponseDto?)null);

        var resultado = await _controller.ObtenerPorId(id);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Registrar_Valido_RetornaCreated()
    {
        var dto = new RegistrarAtencionDto
        {
            PacienteId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            CostoFinal = 100
        };
        var nuevoId = Guid.NewGuid();
        _atencionService.RegistrarAtencionAsync(dto).Returns(nuevoId);

        var resultado = await _controller.Registrar(dto);

        var created = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(AtencionesController.ObtenerPorId));
        created.RouteValues!["id"].Should().Be(nuevoId);
    }

    [Fact]
    public async Task Cerrar_Valido_RetornaOk()
    {
        var id = Guid.NewGuid();
        var dto = new CerrarAtencionDto
        {
            ImpresionDiagnostica = new()
            {
                DiagnosticoPrincipal = "Test",
                IndicacionesReceta = "Receta"
            }
        };

        _atencionService.CerrarAtencionAsync(id, dto).Returns(Task.CompletedTask);

        var resultado = await _controller.Cerrar(id, dto);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Anular_Valido_RetornaOk()
    {
        var id = Guid.NewGuid();
        var motivo = "Motivo de prueba";
        _atencionService.AnularAtencionAsync(id, motivo).Returns(Task.CompletedTask);

        var resultado = await _controller.Anular(id, new AnularAtencionRequest { Motivo = motivo });

        resultado.Should().BeOfType<OkObjectResult>();
    }
}