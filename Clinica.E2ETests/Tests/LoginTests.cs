using System;
using System.Threading.Tasks;
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

        await TakeScreenshotOnFailure(async () =>
        {
            await Page.GotoAsync("/login");
            await Page.WaitForSelectorAsync("[role='progressbar']", new() { State = WaitForSelectorState.Hidden });

            await Page.GotoAsync("/login");
            await Page.WaitForSelectorAsync("[role='progressbar']", new() { State = WaitForSelectorState.Hidden });

            await Page.GetByLabel("Usuario o Correo Electrónico").FillAsync("usuario_invalido");
            await Page.GetByLabel("Contraseña").FillAsync("clave_incorrecta");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Ingresar a Intranet" }).ClickAsync();

            // Esperamos que el MudAlert de error sea visible
            await Page.WaitForSelectorAsync(".mud-alert-filled-error",
                new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

            var mensajeError = await Page.TextContentAsync(".mud-alert-filled-error");
            Assert.Contains("Usuario o contraseña incorrectos", mensajeError, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task Login_MostrarContrasena_ToggleFunciona()
    {
        await Page.GotoAsync("/login");
        await Page.WaitForSelectorAsync("[role='progressbar']", new() { State = WaitForSelectorState.Hidden });

        var passwordInput = Page.GetByLabel("Contraseña");
        Assert.Equal("password", await passwordInput.GetAttributeAsync("type"));

        // Clic en el botón de visibilidad (el adornment del MudTextField)
        await Page.ClickAsync(".mud-input-adornment-icon-button");

        // Después de hacer clic, el input debe ser de tipo "text"
        await Page.WaitForFunctionAsync("() => document.querySelector('input[type=\"text\"]') != null");
        var type = await passwordInput.GetAttributeAsync("type");
        Assert.Equal("text", type);
    }
}