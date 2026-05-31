using Microsoft.Playwright;
using Xunit;

namespace Clinica.E2ETests.Tests;

public class LoginTests : E2ETestBase
{
    [Fact]
    public async Task Login_ConCredencialesValidas_DeberiaRedirigirAlDashboard()
    {
        // 1. Navegar a la página de login
        await Page.GotoAsync("/login");
        
        // 2. Esperar a que Blazor termine de cargar (desaparece un indicador de carga)
        await Page.WaitForSelectorAsync("[role='progressbar']", new() { State = WaitForSelectorState.Hidden });
        
        // 3. Rellenar el formulario
        await Page.FillAsync("input[placeholder='Usuario']", "admin");
        await Page.FillAsync("input[placeholder='Contraseña']", "admin123");
        
        // 4. Hacer clic en el botón de login
        await Page.ClickAsync("button:has-text('Iniciar sesión')");
        
        // 5. Esperar a que aparezca algún elemento del dashboard
        await Page.WaitForSelectorAsync("h1:has-text('Dashboard')");
        
        // 6. Verificar que estamos en la página correcta
        Assert.Contains("/dashboard", Page.Url);
    }
}