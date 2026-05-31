using Microsoft.Playwright;

namespace Clinica.E2ETests.Helpers;

public static class AuthHelper
{
    /// <summary>
    /// Inicia sesión con las credenciales indicadas y espera a que aparezca el dashboard.
    /// </summary>
    public static async Task LoginAsync(IPage page, string usuario, string password)
    {
        await page.GotoAsync("/login");

        // Esperar que Blazor termine de hidratar (desaparece el progreso circular)
        await page.WaitForSelectorAsync("[role='progressbar']", new()
        {
            State = WaitForSelectorState.Hidden
        });

        // Llenar campos usando el label asociado (funciona con MudTextField)
        await page.GetByLabel("Usuario o Correo Electrónico").FillAsync(usuario);
        await page.GetByLabel("Contraseña").FillAsync(password);

        // Hacer clic en el botón "Ingresar a Intranet"
        await page.GetByRole(AriaRole.Button, new() { Name = "Ingresar a Intranet" }).ClickAsync();

        // Esperar a que aparezca algún elemento del dashboard (después de la redirección)
        await page.WaitForURLAsync("**/dashboard", new() { Timeout = 10_000 });
    }
    
    public static async Task LoginAsAdminAsync(IPage page)
    {
        await LoginAsync(page, "admin", "admin123"); // Ajusta la contraseña si es distinta
    }
}