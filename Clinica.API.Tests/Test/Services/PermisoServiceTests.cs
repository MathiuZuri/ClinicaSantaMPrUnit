using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class PermisoServiceTests
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IPermisoService _service;

    public PermisoServiceTests()
    {
        _permisoRepository = Substitute.For<IPermisoRepository>();
        _service = new PermisoService(_permisoRepository);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarDtosMapeados()
    {
        // Arrange
        var permisos = new List<Permiso>
        {
            CrearPermisoEntidad(),
            CrearPermisoEntidad(
                codigo: "PACIENTE_CREAR",
                nombre: "Crear pacientes",
                modulo: "Pacientes")
        };

        _permisoRepository.GetAllAsync().Returns(permisos);

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);

        resultado[0].Id.Should().Be(permisos[0].Id);
        resultado[0].Codigo.Should().Be(permisos[0].Codigo);
        resultado[0].Nombre.Should().Be(permisos[0].Nombre);
        resultado[0].Modulo.Should().Be(permisos[0].Modulo);
        resultado[0].Descripcion.Should().Be(permisos[0].Descripcion);
        resultado[0].Activo.Should().Be(permisos[0].Activo);

        resultado[1].Id.Should().Be(permisos[1].Id);
        resultado[1].Codigo.Should().Be(permisos[1].Codigo);
        resultado[1].Nombre.Should().Be(permisos[1].Nombre);
        resultado[1].Modulo.Should().Be(permisos[1].Modulo);
        resultado[1].Descripcion.Should().Be(permisos[1].Descripcion);
        resultado[1].Activo.Should().Be(permisos[1].Activo);
    }

    [Fact]
    public async Task ObtenerTodosAsync_SiNoHayPermisos_DebeRetornarColeccionVacia()
    {
        // Arrange
        _permisoRepository.GetAllAsync().Returns(Enumerable.Empty<Permiso>());

        // Act
        var resultado = (await _service.ObtenerTodosAsync()).ToList();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    private static Permiso CrearPermisoEntidad(
        string codigo = "PACIENTE_VER",
        string nombre = "Ver pacientes",
        string modulo = "Pacientes")
    {
        return new Permiso
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nombre = nombre,
            Modulo = modulo,
            Descripcion = "Descripción de prueba",
            Activo = true
        };
    }
}