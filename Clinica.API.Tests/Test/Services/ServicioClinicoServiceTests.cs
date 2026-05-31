using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class ServicioClinicoServiceTests
{
    private readonly IServicioClinicoRepository _servicioRepository;
    private readonly IServicioClinicoService _service;

    public ServicioClinicoServiceTests()
    {
        _servicioRepository = Substitute.For<IServicioClinicoRepository>();
        _service = new ServicioClinicoService(_servicioRepository);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var servicios = new List<ServicioClinico>
        {
            CrearServicioEntidad(),
            CrearServicioEntidad()
        };

        _servicioRepository.GetAllAsync().Returns(servicios);

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(servicios[0].Id);
        resultado[0].CodigoServicio.Should().Be(servicios[0].CodigoServicio);
        resultado[0].Nombre.Should().Be(servicios[0].Nombre);
        resultado[0].Descripcion.Should().Be(servicios[0].Descripcion);
        resultado[0].CostoBase.Should().Be(servicios[0].CostoBase);
        resultado[0].DuracionMinutos.Should().Be(servicios[0].DuracionMinutos);
        resultado[0].RequiereCita.Should().Be(servicios[0].RequiereCita);
        resultado[0].GeneraHistorial.Should().Be(servicios[0].GeneraHistorial);
        resultado[0].Estado.Should().Be(servicios[0].Estado);
    }

    [Fact]
    public async Task ObtenerActivosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var servicios = new List<ServicioClinico> { CrearServicioEntidad() };

        _servicioRepository.ObtenerActivosAsync().Returns(servicios);

        // Act
        var resultado = (await _service.ObtenerActivosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].Id.Should().Be(servicios[0].Id);
        resultado[0].CodigoServicio.Should().Be(servicios[0].CodigoServicio);
        resultado[0].Estado.Should().Be(servicios[0].Estado);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiExiste_DebeRetornarDto()
    {
        // Arrange
        var servicio = CrearServicioEntidad();
        _servicioRepository.GetByIdAsync(servicio.Id).Returns(servicio);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(servicio.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(servicio.Id);
        resultado.CodigoServicio.Should().Be(servicio.CodigoServicio);
        resultado.Nombre.Should().Be(servicio.Nombre);
        resultado.Descripcion.Should().Be(servicio.Descripcion);
        resultado.CostoBase.Should().Be(servicio.CostoBase);
        resultado.DuracionMinutos.Should().Be(servicio.DuracionMinutos);
        resultado.RequiereCita.Should().Be(servicio.RequiereCita);
        resultado.GeneraHistorial.Should().Be(servicio.GeneraHistorial);
        resultado.Estado.Should().Be(servicio.Estado);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _servicioRepository.GetByIdAsync(id).Returns((ServicioClinico?)null);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    private static ServicioClinico CrearServicioEntidad(Guid? id = null)
    {
        return new ServicioClinico
        {
            Id = id ?? Guid.NewGuid(),
            CodigoServicio = "CONOBS",
            Nombre = "Consulta obstétrica",
            Descripcion = "Consulta general especializada",
            CostoBase = 80.50m,
            DuracionMinutos = 30,
            RequiereCita = true,
            GeneraHistorial = true,
            Estado = EstadoServicioClinico.Activo
        };
    }
    
    [Fact]
    public async Task ObtenerTodosAsync_CuandoNoHayServicios_RetornaListaVacia()
    {
        _servicioRepository.GetAllAsync().Returns(new List<ServicioClinico>());

        var resultado = await _service.ObtenerTodosAsync();

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerActivosAsync_CuandoNoHayServicios_RetornaListaVacia()
    {
        _servicioRepository.ObtenerActivosAsync().Returns(new List<ServicioClinico>());

        var resultado = await _service.ObtenerActivosAsync();

        resultado.Should().BeEmpty();
    }
}