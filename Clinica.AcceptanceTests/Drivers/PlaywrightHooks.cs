using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;

namespace Clinica.AcceptanceTests.Drivers;

[Binding]
public class PlaywrightHooks
{
    private readonly IObjectContainer _container;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    public PlaywrightHooks(IObjectContainer container)
    {
        _container = container;
    }

    [BeforeScenario]
    public async Task InitializePlaywright()
    {
        // 1. Inicializar el motor de Playwright
        _playwright = await Playwright.CreateAsync();
        
        // 2. Lanzar el navegador (Headless = false permite ver la automatización en vivo)
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, 
            SlowMo = 600 // Añade un retraso de 600ms para que el jurado pueda ver cómo el bot escribe
        });

        // 3. Crear un contexto ignorando los errores de certificados autofirmados de localhost
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        // 4. Crear la página y registrarla en el contenedor de dependencias de Reqnroll
        _page = await context.NewPageAsync();
        _container.RegisterInstanceAs<IPage>(_page);
    }

    [AfterScenario]
    public async Task ClosePlaywright()
    {
        if (_browser is not null) 
            await _browser.CloseAsync();
            
        _playwright?.Dispose();
    }
}