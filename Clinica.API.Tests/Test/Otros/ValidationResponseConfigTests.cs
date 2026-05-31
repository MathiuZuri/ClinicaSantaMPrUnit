using Clinica.API.Configurations;
using Clinica.API.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class ValidationResponseConfigTests
{
    [Fact]
    public void ConfigurarRespuestasDeValidacion_DebeConfigurarInvalidModelStateResponseFactory()
    {
        // Arrange
        var options = new ApiBehaviorOptions();

        // Act
        ValidationResponseConfig.ConfigurarRespuestasDeValidacion(options);

        // Assert
        options.InvalidModelStateResponseFactory.Should().NotBeNull();
    }

    [Fact]
    public void InvalidModelStateResponseFactory_DebeRetornarBadRequestConErroresDistinct()
    {
        // Arrange
        var options = new ApiBehaviorOptions();
        ValidationResponseConfig.ConfigurarRespuestasDeValidacion(options);

        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Campo1", "El campo 1 es obligatorio.");
        modelState.AddModelError("Campo2", "El campo 2 es obligatorio.");
        modelState.AddModelError("Campo3", "El campo 1 es obligatorio.");

        var actionContext = CrearActionContext(modelState);
        var context = new ActionContextWrapper(actionContext);

        // Act
        var result = options.InvalidModelStateResponseFactory!(context);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeFalse();
        response.Codigo.Should().Be(StatusCodes.Status400BadRequest);
        response.Mensaje.Should().Be("La solicitud contiene errores de validación.");
        response.Errores.Should().HaveCount(2);
        response.Errores.Should().Contain("El campo 1 es obligatorio.");
        response.Errores.Should().Contain("El campo 2 es obligatorio.");
    }

    [Fact]
    public void InvalidModelStateResponseFactory_SiErrorMessageEsVacio_DebeUsarMensajePorDefecto()
    {
        // Arrange
        var options = new ApiBehaviorOptions();
        ValidationResponseConfig.ConfigurarRespuestasDeValidacion(options);

        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Campo1", string.Empty);

        var actionContext = CrearActionContext(modelState);
        var context = new ActionContextWrapper(actionContext);

        // Act
        var result = options.InvalidModelStateResponseFactory!(context);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Exitoso.Should().BeFalse();
        response.Codigo.Should().Be(StatusCodes.Status400BadRequest);
        response.Mensaje.Should().Be("La solicitud contiene errores de validación.");
        response.Errores.Should().Contain("Error de validación en la solicitud.");
    }

    [Fact]
    public void InvalidModelStateResponseFactory_DebeRetornarErroresUnicos_AunSiSeRepitenEnDistintosCampos()
    {
        // Arrange
        var options = new ApiBehaviorOptions();
        ValidationResponseConfig.ConfigurarRespuestasDeValidacion(options);

        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Campo1", "Dato inválido.");
        modelState.AddModelError("Campo2", "Dato inválido.");
        modelState.AddModelError("Campo3", "Dato inválido.");

        var actionContext = CrearActionContext(modelState);
        var context = new ActionContextWrapper(actionContext);

        // Act
        var result = options.InvalidModelStateResponseFactory!(context);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        response.Errores.Should().HaveCount(1);
        response.Errores.Single().Should().Be("Dato inválido.");
    }

    private static ActionContext CrearActionContext(ModelStateDictionary modelState)
    {
        return new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            modelState
        );
    }

    private sealed class ActionContextWrapper : ActionContext
    {
        public ActionContextWrapper(ActionContext actionContext)
            : base(
                actionContext.HttpContext,
                actionContext.RouteData,
                actionContext.ActionDescriptor,
                actionContext.ModelState)
        {
        }
    }
}