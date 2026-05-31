using Clinica.API.Helpers;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class FechaHelperTests
{

    [Fact]
    public void ToUtc_FechaEnUtc_RetornaMismaFecha()
    {
        // Arrange
        var fechaUtc = new DateTime(2026, 5, 20, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var resultado = FechaHelper.ToUtc(fechaUtc);

        // Assert
        resultado.Should().Be(fechaUtc);
        resultado.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ToUtc_FechaLocal_ConvierteAUltimoUtc()
    {
        // Arrange
        var fechaLocal = new DateTime(2026, 5, 20, 10, 30, 0, DateTimeKind.Local);
        var esperadoUtc = fechaLocal.ToUniversalTime();

        // Act
        var resultado = FechaHelper.ToUtc(fechaLocal);

        // Assert
        resultado.Should().Be(esperadoUtc);
        resultado.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ToUtc_FechaUnspecified_ConvierteASpecifyKindUtc()
    {
        // Arrange
        var fechaUnspecified = new DateTime(2026, 5, 20, 10, 30, 0, DateTimeKind.Unspecified);
        var esperado = DateTime.SpecifyKind(fechaUnspecified, DateTimeKind.Utc);

        // Act
        var resultado = FechaHelper.ToUtc(fechaUnspecified);

        // Assert
        resultado.Should().Be(esperado);
        resultado.Kind.Should().Be(DateTimeKind.Utc);
    }
    
    [Fact]
    public void ToUtc_Nullable_ConFechaNoNula_RetornaFechaEnUtc()
    {
        // Arrange
        DateTime? fechaNullable = new DateTime(2026, 5, 20, 10, 30, 0, DateTimeKind.Local);
        var esperado = fechaNullable.Value.ToUniversalTime();

        // Act
        var resultado = FechaHelper.ToUtc(fechaNullable);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().Be(esperado);
        resultado!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ToUtc_Nullable_ConNull_RetornaNull()
    {
        // Arrange
        DateTime? fechaNullable = null;

        // Act
        var resultado = FechaHelper.ToUtc(fechaNullable);

        // Assert
        resultado.Should().BeNull();
    }
}