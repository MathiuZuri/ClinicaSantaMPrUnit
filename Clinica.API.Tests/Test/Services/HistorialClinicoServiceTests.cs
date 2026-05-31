using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class HistorialClinicoServiceTests
{
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _detalleRepository;
    private readonly IHistorialClinicoService _service;

    public HistorialClinicoServiceTests()
    {
        _historialRepository = Substitute.For<IHistorialClinicoRepository>();
        _detalleRepository = Substitute.For<IHistorialDetalleRepository>();
        _service = new HistorialClinicoService(_historialRepository, _detalleRepository);
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_SiExiste_DebeRetornarDtoConDetalles()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(pacienteId: pacienteId);
        var detalles = new List<HistorialDetalle>
        {
            CrearDetalleEntidad(historial.Id),
            CrearDetalleEntidad(historial.Id)
        };

        _historialRepository.ObtenerPorPacienteAsync(pacienteId).Returns(historial);
        _detalleRepository.ObtenerPorHistorialAsync(historial.Id).Returns(detalles);

        // Act
        var resultado = await _service.ObtenerPorPacienteAsync(pacienteId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(historial.Id);
        resultado.CodigoHistorial.Should().Be(historial.CodigoHistorial);
        resultado.PacienteId.Should().Be(historial.PacienteId);
        resultado.PacienteNombre.Should().Be($"{historial.Paciente.Nombres} {historial.Paciente.Apellidos}");
        resultado.PacienteDni.Should().Be(historial.Paciente.DNI);
        resultado.FechaApertura.Should().Be(historial.FechaApertura);
        resultado.Estado.Should().Be(historial.Estado);
        resultado.Detalles.Should().HaveCount(2);

        resultado.Detalles[0].Id.Should().Be(detalles[0].Id);
        resultado.Detalles[0].CodigoDetalle.Should().Be(detalles[0].CodigoDetalle);
        resultado.Detalles[0].HistorialClinicoId.Should().Be(detalles[0].HistorialClinicoId);
        resultado.Detalles[0].TipoMovimiento.Should().Be(detalles[0].TipoMovimiento);
        resultado.Detalles[0].CitaId.Should().Be(detalles[0].CitaId);
        resultado.Detalles[0].AtencionId.Should().Be(detalles[0].AtencionId);
        resultado.Detalles[0].PagoId.Should().Be(detalles[0].PagoId);
        resultado.Detalles[0].Titulo.Should().Be(detalles[0].Titulo);
        resultado.Detalles[0].Descripcion.Should().Be(detalles[0].Descripcion);
        resultado.Detalles[0].FechaRegistro.Should().Be(detalles[0].FechaRegistro);
        resultado.Detalles[0].UsuarioId.Should().Be(detalles[0].UsuarioId);
        resultado.Detalles[0].UsuarioNombre.Should().Be($"{detalles[0].Usuario!.Nombres} {detalles[0].Usuario!.Apellidos}");
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        _historialRepository.ObtenerPorPacienteAsync(pacienteId).Returns((HistorialClinico?)null);

        // Act
        var resultado = await _service.ObtenerPorPacienteAsync(pacienteId);

        // Assert
        resultado.Should().BeNull();
        await _detalleRepository.DidNotReceive().ObtenerPorHistorialAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_SiNoTieneDetalles_DebeRetornarListaVacia()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(pacienteId: pacienteId);

        _historialRepository.ObtenerPorPacienteAsync(pacienteId).Returns(historial);
        _detalleRepository.ObtenerPorHistorialAsync(historial.Id).Returns(Enumerable.Empty<HistorialDetalle>());

        // Act
        var resultado = await _service.ObtenerPorPacienteAsync(pacienteId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Detalles.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_SiHistorialNoTienePaciente_DebeMapearNombreYDniVacios()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(pacienteId: pacienteId);
        historial.Paciente = null!;

        _historialRepository.ObtenerPorPacienteAsync(pacienteId).Returns(historial);
        _detalleRepository.ObtenerPorHistorialAsync(historial.Id).Returns(Enumerable.Empty<HistorialDetalle>());

        // Act
        var resultado = await _service.ObtenerPorPacienteAsync(pacienteId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.PacienteNombre.Should().BeEmpty();
        resultado.PacienteDni.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerConDetallesAsync_SiExiste_DebeRetornarDtoConDetallesDelHistorial()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(id: historialId);
        historial.Detalles.Add(CrearDetalleEntidad(historialId));
        historial.Detalles.Add(CrearDetalleEntidad(historialId));

        _historialRepository.ObtenerConDetallesAsync(historialId).Returns(historial);

        // Act
        var resultado = await _service.ObtenerConDetallesAsync(historialId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(historial.Id);
        resultado.CodigoHistorial.Should().Be(historial.CodigoHistorial);
        resultado.PacienteId.Should().Be(historial.PacienteId);
        resultado.PacienteNombre.Should().Be($"{historial.Paciente.Nombres} {historial.Paciente.Apellidos}");
        resultado.PacienteDni.Should().Be(historial.Paciente.DNI);
        resultado.Detalles.Should().HaveCount(2);

        resultado.Detalles[0].HistorialClinicoId.Should().Be(historialId);
        resultado.Detalles[0].UsuarioNombre.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerConDetallesAsync_SiNoExiste_DebeRetornarNull()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        _historialRepository.ObtenerConDetallesAsync(historialId).Returns((HistorialClinico?)null);

        // Act
        var resultado = await _service.ObtenerConDetallesAsync(historialId);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerConDetallesAsync_SiNoTieneDetalles_DebeRetornarListaVacia()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(id: historialId);

        _historialRepository.ObtenerConDetallesAsync(historialId).Returns(historial);

        // Act
        var resultado = await _service.ObtenerConDetallesAsync(historialId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Detalles.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorPacienteAsync_SiDetalleNoTieneUsuario_DebeMapearUsuarioNombreNull()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(pacienteId: pacienteId);
        var detalle = CrearDetalleEntidad(historial.Id);
        detalle.Usuario = null;
        detalle.UsuarioId = null;

        _historialRepository.ObtenerPorPacienteAsync(pacienteId).Returns(historial);
        _detalleRepository.ObtenerPorHistorialAsync(historial.Id).Returns(new[] { detalle });

        // Act
        var resultado = await _service.ObtenerPorPacienteAsync(pacienteId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Detalles.Should().HaveCount(1);
        resultado.Detalles[0].UsuarioId.Should().BeNull();
        resultado.Detalles[0].UsuarioNombre.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerConDetallesAsync_DebeMapearReferenciasOpcionalesDelDetalle()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(id: historialId);
        var detalle = CrearDetalleEntidad(historialId);
        detalle.CitaId = Guid.NewGuid();
        detalle.AtencionId = Guid.NewGuid();
        detalle.PagoId = Guid.NewGuid();

        historial.Detalles.Add(detalle);

        _historialRepository.ObtenerConDetallesAsync(historialId).Returns(historial);

        // Act
        var resultado = await _service.ObtenerConDetallesAsync(historialId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Detalles.Should().HaveCount(1);
        resultado.Detalles[0].CitaId.Should().Be(detalle.CitaId);
        resultado.Detalles[0].AtencionId.Should().Be(detalle.AtencionId);
        resultado.Detalles[0].PagoId.Should().Be(detalle.PagoId);
    }

    private static HistorialClinico CrearHistorialEntidad(Guid? id = null, Guid? pacienteId = null)
    {
        var pId = pacienteId ?? Guid.NewGuid();

        return new HistorialClinico
        {
            Id = id ?? Guid.NewGuid(),
            CodigoHistorial = "ABCDE-2026-12345678",
            PacienteId = pId,
            Paciente = new Paciente
            {
                Id = pId,
                DNI = "12345678",
                Nombres = "Ana",
                Apellidos = "Quispe"
            },
            FechaApertura = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc),
            Estado = EstadoHistorialClinico.Activo
        };
    }

    private static HistorialDetalle CrearDetalleEntidad(Guid historialId)
    {
        return new HistorialDetalle
        {
            Id = Guid.NewGuid(),
            CodigoDetalle = "ABCDE-REG-2026-12345678",
            HistorialClinicoId = historialId,
            TipoMovimiento = TipoMovimientoHistorial.AperturaHistorial,
            CitaId = null,
            AtencionId = null,
            PagoId = null,
            Titulo = "Apertura de historial clínico",
            Descripcion = "Se aperturó el historial clínico del paciente.",
            FechaRegistro = new DateTime(2026, 1, 10, 9, 5, 0, DateTimeKind.Utc),
            UsuarioId = Guid.NewGuid(),
            Usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombres = "Carlos",
                Apellidos = "Mamani"
            }
        };
    }
    
    [Fact]
    public async Task ObtenerConDetallesAsync_CuandoPacienteEsNull_DebeMapearNombreYDniVacios()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(id: historialId);
        historial.Paciente = null!; // fuerza la rama null

        _historialRepository.ObtenerConDetallesAsync(historialId).Returns(historial);

        // Act
        var resultado = await _service.ObtenerConDetallesAsync(historialId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.PacienteNombre.Should().BeEmpty();
        resultado.PacienteDni.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerConDetallesAsync_CuandoDetalleSinUsuario_DebeMapearUsuarioNombreNull()
    {
        // Arrange
        var historialId = Guid.NewGuid();
        var historial = CrearHistorialEntidad(id: historialId);
        var detalle = CrearDetalleEntidad(historialId);
        detalle.Usuario = null;
        detalle.UsuarioId = null;

        historial.Detalles.Add(detalle);

        _historialRepository.ObtenerConDetallesAsync(historialId).Returns(historial);

        // Act
        var resultado = await _service.ObtenerConDetallesAsync(historialId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Detalles.Should().HaveCount(1);
        resultado.Detalles[0].UsuarioId.Should().BeNull();
        resultado.Detalles[0].UsuarioNombre.Should().BeNull();
    }
}