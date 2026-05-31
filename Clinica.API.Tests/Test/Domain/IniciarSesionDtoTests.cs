using System.ComponentModel.DataAnnotations;
using Clinica.Domain.DTOs.Auth;
using FluentAssertions;
using Xunit;

namespace Clinica.API.Tests.Test.Domain;

public class IniciarSesionDtoTests
{
    [Fact]
    public void ConstructorVacio_DebeInicializarValoresPorDefecto()
    {
        // Act
        var dto = new IniciarSesionDto();

        // Assert
        dto.UsuarioOCorreo.Should().BeEmpty();
        dto.Password.Should().BeEmpty();
    }

    [Fact]
    public void DataAnnotations_SiDtoEsValido_NoDebeRetornarErrores()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin@correo.com",
            Password = "Password123"
        };

        // Act
        var resultados = ValidarModelo(dto);

        // Assert
        resultados.Should().BeEmpty();
    }

    [Fact]
    public void DataAnnotations_SiUsuarioOCorreoEsVacio_DebeRetornarError()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "",
            Password = "Password123"
        };

        // Act
        var resultados = ValidarModelo(dto);

        // Assert
        resultados.Should().Contain(x => x.MemberNames.Contains(nameof(IniciarSesionDto.UsuarioOCorreo)));
    }

    [Fact]
    public void DataAnnotations_SiPasswordEsMuyCorta_DebeRetornarError()
    {
        // Arrange
        var dto = new IniciarSesionDto
        {
            UsuarioOCorreo = "kevin",
            Password = "123"
        };

        // Act
        var resultados = ValidarModelo(dto);

        // Assert
        resultados.Should().Contain(x => x.MemberNames.Contains(nameof(IniciarSesionDto.Password)));
    }

    private static List<ValidationResult> ValidarModelo(object modelo)
    {
        var contexto = new ValidationContext(modelo);
        var resultados = new List<ValidationResult>();

        Validator.TryValidateObject(modelo, contexto, resultados, validateAllProperties: true);

        return resultados;
    }
}