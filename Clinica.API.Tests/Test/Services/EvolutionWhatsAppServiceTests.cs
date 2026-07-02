using System.Net;
using System.Text;
using Clinica.API.Configurations;
using Clinica.API.Services;
using Clinica.API.Services.Imp;
using Clinica.API.Services.Imp.WhastAppImp;
using Clinica.API.Services.Imp.WhatsApp;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class EvolutionWhatsAppServiceTests
{
    [Fact]
    public async Task EnviarMensajeAsync_SiWhatsAppEstaDeshabilitado_NoDebeEnviarRequest()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler((_, _) =>
            throw new InvalidOperationException("No debería llamarse al HttpClient."));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test")
        };

        var options = Options.Create(new WhatsAppOptions
        {
            Enabled = false,
            InstanceName = "clinica-santa-monica",
            ApiKey = "test-key"
        });

        var logger = Substitute.For<ILogger<EvolutionWhatsAppService>>();
        INotificacionWhatsAppService service = new EvolutionWhatsAppService(httpClient, options, logger);

        // Act
        var act = async () => await service.EnviarMensajeAsync("987654321", "Hola");

        // Assert
        await act.Should().NotThrowAsync();
        handler.CallCount.Should().Be(0);
    }

    [Theory]
    [InlineData("987654321", "51987654321")]
    [InlineData("+51 987 654 321", "51987654321")]
    [InlineData("51-987-654-321", "51987654321")]
    [InlineData("(987)654321", "51987654321")]
    public async Task EnviarMensajeAsync_DebeNormalizarTelefonoPeruCorrectamente(string telefonoEntrada, string esperado)
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        var handler = new FakeHttpMessageHandler(async (request, _) =>
        {
            capturedRequest = request;
            capturedBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"success":true}""", Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test")
        };

        var options = Options.Create(new WhatsAppOptions
        {
            Enabled = true,
            InstanceName = "clinica",
            ApiKey = "test-key"
        });

        var logger = Substitute.For<ILogger<EvolutionWhatsAppService>>();
        INotificacionWhatsAppService service = new EvolutionWhatsAppService(httpClient, options, logger);

        // Act
        await service.EnviarMensajeAsync(telefonoEntrada, "Mensaje de prueba");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri.Should().NotBeNull();
        capturedRequest.RequestUri!.AbsolutePath.Should().Be("/message/sendText/clinica");

        capturedRequest.Headers.Contains("apikey").Should().BeTrue();
        capturedRequest.Headers.GetValues("apikey").Single().Should().Be("test-key");

        capturedBody.Should().NotBeNull();
        capturedBody.Should().Contain(esperado);
        capturedBody.Should().Contain("Mensaje de prueba");
    }

    [Fact]
    public async Task EnviarMensajeAsync_SiRespuestaEsExitosa_NoDebeLanzarExcepcion()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"status":"ok"}""", Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test")
        };

        var options = Options.Create(new WhatsAppOptions
        {
            Enabled = true,
            InstanceName = "clinica",
            ApiKey = "test-key"
        });

        var logger = Substitute.For<ILogger<EvolutionWhatsAppService>>();
        INotificacionWhatsAppService service = new EvolutionWhatsAppService(httpClient, options, logger);

        // Act
        var act = async () => await service.EnviarMensajeAsync("987654321", "Hola");

        // Assert
        await act.Should().NotThrowAsync();
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task EnviarMensajeAsync_SiRespuestaNoEsExitosa_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":"bad request"}""", Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test")
        };

        var options = Options.Create(new WhatsAppOptions
        {
            Enabled = true,
            InstanceName = "clinica",
            ApiKey = "test-key"
        });

        var logger = Substitute.For<ILogger<EvolutionWhatsAppService>>();
        INotificacionWhatsAppService service = new EvolutionWhatsAppService(httpClient, options, logger);

        // Act
        Func<Task> act = async () => await service.EnviarMensajeAsync("987654321", "Hola");

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Error enviando mensaje por WhatsApp: BadRequest");
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public int CallCount { get; private set; }

        public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return _handler(request, cancellationToken);
        }
    }
}