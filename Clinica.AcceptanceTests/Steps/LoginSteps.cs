using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll; // Asegúrate de tener este using activo

namespace Clinica.AcceptanceTests.Steps;

[Binding]
public class LoginSteps
{
    private readonly IPage _page;
    private const string BaseUrl = "https://localhost:7299"; 

    public LoginSteps(IPage page)
    {
        _page = page;
    }

    // Cambiar [Dado] por [Given]
    [Given(@"que el usuario navega a la página de inicio de sesión")]
    public async Task DadoQueElUsuarioNavegaALaPaginaDeInicioDeSesion()
    {
        await _page.GotoAsync($"{BaseUrl}/login");
    }

    // Cambiar [Cuando] por [When]
    [When(@"ingresa el usuario o correo ""(.*)""")]
    public async Task CuandoIngresaElUsuarioOCorreo(string usuario)
    {
        await _page.GetByLabel("Usuario o Correo Electrónico").FillAsync(usuario);
    }

    [When(@"digita la contraseña ""(.*)""")]
    public async Task CuandoDigitaLaContrasena(string password)
    {
        await _page.GetByLabel("Contraseña").FillAsync(password);
    }

    [When(@"hace clic en el botón principal ""(.*)""")]
    public async Task CuandoHaceClicEnElBotonPrincipal(string nombreBoton)
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = nombreBoton }).ClickAsync();
    }

    // Cambiar [Entonces] por [Then]
    [Then(@"el sistema debe redirigirlo automáticamente al panel principal ""(.*)""")]
    public async Task EntoncesElSistemaDebeRedirigirloAlPanelPrincipal(string rutaEsperada)
    {
        await _page.WaitForURLAsync($"{BaseUrl}/{rutaEsperada}");
        Assert.That(_page.Url, Does.Contain(rutaEsperada));
    }

    [Then(@"el sistema debe mostrar un mensaje de alerta con el texto ""(.*)""")]
    public async Task EntoncesElSistemaDebeMostrarUnMensajeDeAlertaConElTexto(string mensajeEsperado)
    {
        var alertaError = _page.Locator(".mud-alert-message");
        await alertaError.WaitForAsync();
        string? textoObtenido = await alertaError.TextContentAsync();
        
        Assert.That(textoObtenido?.Trim(), Is.EqualTo(mensajeEsperado));
    }
}