using System.Reflection;
using System.Security.Claims;
using Clinica.API.Filters;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class AuditoriaAutomaticaFilterTests
{
    private ApplicationDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Constructor_DebeInicializarContexto()
    {
        using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);
        Assert.NotNull(filter);
    }

    [Fact]
    public async Task OnActionExecutionAsync_MetodoGet_GeneraAuditoriaConsulta()
    {
        // Arrange
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        // Usamos un método sin atributo para que el filtro use la detección automática (GET -> Consulta)
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Pacientes",
            ActionName = "GetAll",
            MethodInfo = methodInfo
        });

        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)
            {
                Result = new OkObjectResult(new { id = Guid.NewGuid() }),
                Exception = null
            };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        // Act
        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        // Assert
        var auditoria = context.Auditorias.Single();
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Consulta);
        auditoria.Nivel.Should().Be(NivelAuditoria.Normal);
        auditoria.EsConsulta.Should().BeTrue();
        auditoria.IpAddress.Should().Be("192.168.1.1");
        auditoria.FueExitoso.Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_MetodoPost_ConAtributo_GeneraAuditoriaCreacion()
    {
        // Arrange
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");

        // Método con atributo de creación
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoConAtributoCreacion))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = nameof(TestController.MetodoConAtributoCreacion),
            MethodInfo = methodInfo
        });

        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);

        actionContext.RouteData.Values["id"] = Guid.NewGuid().ToString();

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)
            {
                Result = new OkObjectResult(new { id = Guid.NewGuid() }),
                Exception = null
            };
            httpContext.Response.StatusCode = 201;
            return Task.FromResult(ctx);
        }

        // Act
        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        // Assert
        var auditoria = context.Auditorias.Single();
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Creacion);
        auditoria.Nivel.Should().Be(NivelAuditoria.Importante);
        auditoria.Modulo.Should().Be("ModuloTest");
        auditoria.EntidadAfectada.Should().Be("EntidadTest");
        auditoria.IpAddress.Should().Be("10.0.0.1");
        auditoria.FueExitoso.Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_IpForwardedFor_UsaForwardedFor()
    {
        // Arrange
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.1, 10.0.0.2";
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = "Get",
            MethodInfo = methodInfo
        });

        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)
            {
                Result = new OkResult()
            };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        // Act
        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        // Assert
        var auditoria = context.Auditorias.Single();
        auditoria.IpAddress.Should().Be("203.0.113.1");
    }

    [Fact]
    public async Task OnActionExecutionAsync_Put_ObtieneSnapshotYAsignaEdicion()
    {
        // Arrange
        await using var context = CrearContexto();
        var paciente = new Paciente { Id = Guid.NewGuid(), Nombres = "Juan", Apellidos = "Perez", DNI = "12345678" };
        context.Pacientes.Add(paciente);
        await context.SaveChangesAsync();

        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "PUT";

        // Método con atributo de edición
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoConAtributoEdicion))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Pacientes",
            ActionName = "Update",
            MethodInfo = methodInfo
        });

        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);

        actionContext.RouteData.Values["id"] = paciente.Id.ToString();

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)
            {
                Result = new OkObjectResult(new { id = paciente.Id })
            };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        // Act
        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        // Assert
        var auditoria = context.Auditorias.Single();
        auditoria.ValorAnterior.Should().NotBeNull();
        auditoria.ValorNuevo.Should().NotBeNull();
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Edicion);
    }

    [Fact]
    public async Task OnActionExecutionAsync_MetodoConUsuarioAutenticado_AsignaUsuarioId()
    {
        // Arrange
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var usuarioId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
        }, "Test"));
        httpContext.User = claims;

        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = "Get",
            MethodInfo = methodInfo
        });

        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)
            {
                Result = new OkResult()
            };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        // Act
        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        // Assert
        var auditoria = context.Auditorias.Single();
        auditoria.UsuarioId.Should().Be(usuarioId);
    }
    
    [Fact]
    public async Task OnActionExecutionAsync_MetodoPutSinAtributo_GeneraEdicion()
    {
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "PUT";
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = "Update",
            MethodInfo = methodInfo
        });

        var actionExecutingContext = new ActionExecutingContext(
            actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)
            {
                Result = new OkResult()
            };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        var auditoria = context.Auditorias.Single();
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Edicion);   // PUT → Edicion
        auditoria.Nivel.Should().Be(NivelAuditoria.Importante);          // PUT → Importante
    }

    [Fact]
    public async Task OnActionExecutionAsync_MetodoPatchSinAtributo_GeneraEdicion()
    {
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "PATCH";
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = "Patch",
            MethodInfo = methodInfo
        });
        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null) { Result = new OkResult() };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        var auditoria = context.Auditorias.Single();
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Edicion);   // PATCH → Edicion
        auditoria.Nivel.Should().Be(NivelAuditoria.Importante);
    }

    [Fact]
    public async Task OnActionExecutionAsync_MetodoDeleteSinAtributo_GeneraEliminacion()
    {
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "DELETE";
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = "Delete",
            MethodInfo = methodInfo
        });
        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null) { Result = new OkResult() };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        var auditoria = context.Auditorias.Single();
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Eliminacion); // DELETE → Eliminacion
        auditoria.Nivel.Should().Be(NivelAuditoria.Critico);                // DELETE → Critico
    }

    [Fact]
    public async Task OnActionExecutionAsync_MetodoDesconocido_DefaultAConsulta()
    {
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "OPTIONS";        // verbo no contemplado
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = "Options",
            MethodInfo = methodInfo
        });
        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null) { Result = new OkResult() };
            httpContext.Response.StatusCode = 200;
            return Task.FromResult(ctx);
        }

        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        var auditoria = context.Auditorias.Single();
        auditoria.TipoAccion.Should().Be(TipoAccionAuditoria.Consulta);  // default → Consulta
        auditoria.Nivel.Should().Be(NivelAuditoria.Normal);              // default → Normal
    }
    
    [Fact]
    public async Task OnActionExecutionAsync_EjecucionFallida_RegistraFallo()
    {
        await using var context = CrearContexto();
        var filter = new AuditoriaAutomaticaFilter(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MetodoSinAtributo))!;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor
        {
            ControllerName = "Test",
            ActionName = "Get",
            MethodInfo = methodInfo
        });
        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);

        Task<ActionExecutedContext> Next()
        {
            // Simulamos excepción
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)
            {
                Exception = new Exception("Error simulado"),
                Result = new ObjectResult("Error") { StatusCode = 500 }
            };
            httpContext.Response.StatusCode = 500;
            return Task.FromResult(ctx);
        }

        await filter.OnActionExecutionAsync(actionExecutingContext, Next);

        var auditoria = context.Auditorias.Single();
        auditoria.FueExitoso.Should().BeFalse();
        auditoria.Descripcion.Should().Contain("fallida"); // la rama false de GenerarDescripcion
    }
    
        private static T InvokePrivateStatic<T>(string methodName, params object[] args)
    {
        var method = typeof(AuditoriaAutomaticaFilter).GetMethod(methodName,
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        return (T)method!.Invoke(null, args)!;
    }

    [Fact]
    public void ObtenerEntidadId_TodasLasRutas_ExtraenGuidCorrecto()
    {
        // id (ya probado)
        var context = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["id"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", context).Should().NotBeNull();

        // comprobanteId
        var ctx2 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["comprobanteId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx2).Should().NotBeNull();

        // pagoId
        var ctx3 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["pagoId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx3).Should().NotBeNull();

        // usuarioId
        var ctx4 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["usuarioId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx4).Should().NotBeNull();

        // pacienteId
        var ctx5 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["pacienteId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx5).Should().NotBeNull();

        // doctorId
        var ctx6 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["doctorId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx6).Should().NotBeNull();

        // citaId
        var ctx7 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["citaId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx7).Should().NotBeNull();

        // atencionId
        var ctx8 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["atencionId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx8).Should().NotBeNull();

        // historialId
        var ctx9 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["historialId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx9).Should().NotBeNull();

        // historialClinicoId
        var ctx10 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["historialClinicoId"] = Guid.NewGuid().ToString() }), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx10).Should().NotBeNull();

        // sin ruta → null
        var ctx11 = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null);
        InvokePrivateStatic<Guid?>("ObtenerEntidadId", ctx11).Should().BeNull();
    }

    [Fact]
    public void BuscarGuidPorNombre_TodasLasRamas()
    {
        // objeto null
        InvokePrivateStatic<Guid?>("BuscarGuidPorNombre", null, "id").Should().BeNull();

        // propiedad directa con valor Guid
        var objConGuid = new { id = Guid.NewGuid() };
        InvokePrivateStatic<Guid?>("BuscarGuidPorNombre", objConGuid, "id").Should().Be(objConGuid.id);

        // propiedad directa con string que se puede parsear a Guid
        var guidStr = Guid.NewGuid().ToString();
        var objConStr = new { id = guidStr };
        InvokePrivateStatic<Guid?>("BuscarGuidPorNombre", objConStr, "id").Should().Be(Guid.Parse(guidStr));

        // propiedad directa con valor no convertible a Guid (TryParse falla)
        var objNoGuid = new { id = "no_es_guid" };
        InvokePrivateStatic<Guid?>("BuscarGuidPorNombre", objNoGuid, "id").Should().BeNull();

        // sin propiedad directa, pero con propiedad Data (ApiResponse)
        var data = new { UsuarioId = Guid.NewGuid() };
        var apiResponse = new { Data = data };
        InvokePrivateStatic<Guid?>("BuscarGuidPorNombre", apiResponse, "UsuarioId").Should().Be(data.UsuarioId);

        // propiedad Data es null
        var apiResponseNullData = new { Data = (object?)null };
        InvokePrivateStatic<Guid?>("BuscarGuidPorNombre", apiResponseNullData, "id").Should().BeNull();
    }
}

// Controlador de prueba con varios métodos para exponer atributos específicos
public class TestController : ControllerBase
{
    [Auditoria("ModuloTest", "EntidadTest", TipoAccionAuditoria.Creacion, NivelAuditoria.Importante)]
    public IActionResult MetodoConAtributoCreacion() => Ok();

    [Auditoria("OtroModulo", "OtraEntidad", TipoAccionAuditoria.Edicion, NivelAuditoria.Importante)]
    public IActionResult MetodoConAtributoEdicion() => Ok();

    public IActionResult MetodoSinAtributo() => Ok();
}