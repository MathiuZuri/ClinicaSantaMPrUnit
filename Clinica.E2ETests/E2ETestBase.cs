using Microsoft.Playwright;

namespace Clinica.E2ETests;

public abstract class E2ETestBase : IAsyncLifetime
{
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, // Cambia a true para CI
            SlowMo = 100       // Opcional, para ver las acciones
        });

        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = "https://localhost:7299", // URL de tu Blazor WASM
            IgnoreHTTPSErrors = true            // Si usas certificado de desarrollo
        });

        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}