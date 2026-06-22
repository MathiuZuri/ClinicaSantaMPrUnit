using Clinica.API.Controllers.pdfControladores;
using Clinica.API.Models;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Domain.PDFsDto;
using Clinica.Domain.PDFsDto.Interfacespdf;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Controllers;

public class CertificadosControllerTests
{
    private readonly ICertificadoTrabajoPdfService _pdfService = Substitute.For<ICertificadoTrabajoPdfService>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IDoctorRepository _doctorRepo = Substitute.For<IDoctorRepository>();
    private readonly IUsuarioActualService _usuarioActual = Substitute.For<IUsuarioActualService>();
    private readonly CertificadosController _controller;

    public CertificadosControllerTests()
    {
        _controller = new CertificadosController(_pdfService, _usuarioRepo, _doctorRepo, _usuarioActual);
    }

    [Fact]
    public void Constructor_DebeInicializar() => Assert.NotNull(_controller);

    [Fact]
    public async Task GenerarCertificadosEnBloque_ConUsuarioIds_GeneraZip()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nombres = "Ana",
            Apellidos = "Prueba",
            CodigoUsuario = "USR-001",
            Correo = "ana@test.com",
            FechaRegistro = DateTime.UtcNow,
            UsuarioRoles = new List<UsuarioRol>
            {
                new() { Activo = true, Rol = new Rol { Nombre = "Doctor" } }
            }
        };

        var request = new CertificadoBlockRequest { UsuarioIds = new List<Guid> { usuarioId } };

        _usuarioRepo.GetByIdAsync(usuarioId).Returns(usuario);
        _usuarioRepo.ObtenerConRolesAsync(usuarioId).Returns(usuario);
        _doctorRepo.GetAllAsync().Returns(new List<Doctor>());
        _pdfService.GeneratePdf(Arg.Any<CertificadoTrabajoDto>()).Returns(new byte[] { 1, 2, 3 });

        var result = await _controller.GenerarCertificadosEnBloque(request);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/zip");
        fileResult.FileContents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerarCertificadosEnBloque_SinUsuariosNiRol_RetornaBadRequest()
    {
        var result = await _controller.GenerarCertificadosEnBloque(new CertificadoBlockRequest());
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var apiResponse = badRequest.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        apiResponse.Mensaje.Should().Contain("Debe especificar al menos un usuario o un rol");
    }
}