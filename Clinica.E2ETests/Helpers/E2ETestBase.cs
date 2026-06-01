using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace Clinica.E2ETests.Helpers;

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
            Headless = false,          // false si quieres ver el navegador
            SlowMo = 0
        });

        // Configurar grabación de video en el contexto
        var contextOptions = new BrowserNewContextOptions
        {
            BaseURL = "https://localhost:7299",
            IgnoreHTTPSErrors = true,
            RecordVideoDir = "videos"   // Carpeta donde se guardarán los videos
        };

        Context = await Browser.NewContextAsync(contextOptions);
        Page = await Context.NewPageAsync();
        
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = "https://localhost:7299",
            IgnoreHTTPSErrors = true,
            RecordVideoDir = "videos"   // Carpeta donde se guardan los videos
        });
    }

    public async Task DisposeAsync()
    {
        // Al cerrar el contexto se finaliza el video automáticamente
        await Context.CloseAsync();  // Esto guarda el archivo .webm en la carpeta videos
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
    
    protected async Task TakeScreenshotOnFailure(Func<Task> testAction)
    {
        try
        {
            await testAction();
        }
        catch
        {
            // Crear carpeta si no existe
            var screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            Directory.CreateDirectory(screenshotsDir);
            var screenshotPath = Path.Combine(screenshotsDir, $"{Guid.NewGuid()}.png");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            throw; // relanzar la excepción para que el test falle
        }
    }
}