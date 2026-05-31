using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class PacienteTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var paciente = new Paciente();

        // Assert
        paciente.Id.Should().NotBeEmpty();
        paciente.CodigoPaciente.Should().BeEmpty();
        paciente.DNI.Should().BeEmpty();
        paciente.Nombres.Should().BeEmpty();
        paciente.Apellidos.Should().BeEmpty();
        paciente.FechaNacimiento.Should().Be(default);
        paciente.Sexo.Should().BeEmpty();

        paciente.Celular.Should().BeNull();
        paciente.Correo.Should().BeNull();
        paciente.Direccion.Should().BeNull();

        paciente.Estado.Should().Be(EstadoPaciente.Activo);
        paciente.UsuarioId.Should().BeEmpty();
        paciente.Usuario.Should().BeNull();
        paciente.FechaRegistro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        paciente.HistorialClinico.Should().BeNull();

        paciente.Citas.Should().NotBeNull().And.BeEmpty();
        paciente.Atenciones.Should().NotBeNull().And.BeEmpty();
        paciente.Pagos.Should().NotBeNull().And.BeEmpty();
        paciente.Comprobantes.Should().NotBeNull().And.BeEmpty();
        paciente.NotificacionesCita.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ConstructorVacio_DebePermitirAsignarPropiedades()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var fechaNacimiento = new DateTime(2000, 5, 10, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var paciente = new Paciente
        {
            CodigoPaciente = "PCT-2026-ABCDE-12345678",
            DNI = "12345678",
            Nombres = "Ana",
            Apellidos = "Quispe",
            FechaNacimiento = fechaNacimiento,
            Sexo = "F",
            Celular = "987654321",
            Correo = "ana@correo.com",
            Direccion = "Juliaca",
            Estado = EstadoPaciente.Inactivo,
            UsuarioId = usuarioId
        };

        // Assert
        paciente.CodigoPaciente.Should().Be("PCT-2026-ABCDE-12345678");
        paciente.DNI.Should().Be("12345678");
        paciente.Nombres.Should().Be("Ana");
        paciente.Apellidos.Should().Be("Quispe");
        paciente.FechaNacimiento.Should().Be(fechaNacimiento);
        paciente.Sexo.Should().Be("F");
        paciente.Celular.Should().Be("987654321");
        paciente.Correo.Should().Be("ana@correo.com");
        paciente.Direccion.Should().Be("Juliaca");
        paciente.Estado.Should().Be(EstadoPaciente.Inactivo);
        paciente.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void Colecciones_DebenPermitirAgregarElementos()
    {
        // Arrange
        var paciente = new Paciente();

        // Act
        paciente.Citas.Add(new Cita());
        paciente.Atenciones.Add(new Atencion());
        paciente.Pagos.Add(new Pago());
        paciente.Comprobantes.Add(new Comprobante());
        paciente.NotificacionesCita.Add(new NotificacionCita());

        // Assert
        paciente.Citas.Should().HaveCount(1);
        paciente.Atenciones.Should().HaveCount(1);
        paciente.Pagos.Should().HaveCount(1);
        paciente.Comprobantes.Should().HaveCount(1);
        paciente.NotificacionesCita.Should().HaveCount(1);
    }

    [Fact]
    public void Estado_DebePoderCambiar()
    {
        // Arrange
        var paciente = new Paciente();

        // Act
        paciente.Estado = EstadoPaciente.Bloqueado;
        paciente.Estado = EstadoPaciente.Eliminado;

        // Assert
        paciente.Estado.Should().Be(EstadoPaciente.Eliminado);
    }
}