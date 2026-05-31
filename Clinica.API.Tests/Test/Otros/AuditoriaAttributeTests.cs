using Clinica.Domain.Validations;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class NotEmptyGuidAttributeTests
{
    private readonly NotEmptyGuidAttribute _attribute = new();

    [Fact]
    public void IsValid_SiEsGuidValidoYNoVacio_DebeRetornarTrue()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var resultado = _attribute.IsValid(value);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void IsValid_SiEsGuidEmpty_DebeRetornarFalse()
    {
        // Act
        var resultado = _attribute.IsValid(Guid.Empty);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void IsValid_SiEsNull_DebeRetornarFalse()
    {
        // Act
        var resultado = _attribute.IsValid(null);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void IsValid_SiNoEsGuid_DebeRetornarFalse()
    {
        // Act
        var resultado = _attribute.IsValid("no-es-guid");

        // Assert
        resultado.Should().BeFalse();
    }
}