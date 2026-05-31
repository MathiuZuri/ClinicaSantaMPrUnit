using Clinica.E2ETests.Helpers;
using Microsoft.Playwright;
using Xunit;

namespace Clinica.E2ETests.Tests;

public class LoginTests : E2ETestBase
{
    [Fact]
    public async Task Login_CredencialesValidas_DeberiaRedirigirAlDashboard()
    {
        await AuthHelper.LoginAsAdminAsync(Page);
        // Si no se lanza TimeoutException, la URL es /dashboard
        Assert.EndsWith("/dashboard", Page.Url);
    }

    [Fact]
    public async Task Login_CredencialesInvalidas_DeberiaMostrarError()
    {
        await Page.GotoAsync("/login");
        await Page.WaitForSelectorAsync("[role='progressbar']", new() { State = WaitForSelectorState.Hidden });

        await Page.GetByLabel("Usuario o Correo Electrónico").FillAsync("usuario_invalido");
        await Page.GetByLabel("Contraseña").FillAsync("clave_incorrecta");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ingresar a Intranet" }).ClickAsync();

        // Esperar que aparezca el MudAlert de error (contenedor con role="alert")
        await Page.WaitForSelectorAsync("[role='alert']", new() { Timeout = 5_000 });
        var mensajeError = await Page.TextContentAsync("[role='alert']");
        Assert.Contains("credenciales", mensajeError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_MostrarContrasena_ToggleFunciona()
    {
        await Page.GotoAsync("/login");
        await Page.WaitForSelectorAsync("[role='progressbar']", new() { State = WaitForSelectorState.Hidden });

        // El campo de contraseña inicialmente tiene type="password"
        var passwordInput = Page.GetByLabel("Contraseña");
        Assert.Equal("password", await passwordInput.GetAttributeAsync("type"));

        // Hacer clic en el ícono de visibilidad (MudIcon con Icon="Visibility")
        await Page.ClickAsync("[data-icon='visibility']"); // Selector aproximado, se puede ajustar

        // Ahora debería ser type="text"
        Assert.Equal("text", await passwordInput.GetAttributeAsync("type"));
    }

    [Fact]
    public async Task Login_ModoOscuro_ToggleFunciona()
    {
        await Page.GotoAsync("/login");
        await Page.WaitForSelectorAsync("[role='progressbar']", new() { State = WaitForSelectorState.Hidden });

        // El switch "Interfaz Noche" es un MudSwitch
        var darkSwitch = Page.GetByLabel("Interfaz Noche");
        Assert.NotNull(darkSwitch);

        // Hacer clic para activar/desactivar (podemos verificar la clase del body)
        await darkSwitch.ClickAsync();
        // Esperar que el tema oscuro se aplique (alguna clase CSS específica de MudBlazor)
        await Page.WaitForSelectorAsync(".mud-theme-dark", new() { Timeout = 5_000 });
    }
}