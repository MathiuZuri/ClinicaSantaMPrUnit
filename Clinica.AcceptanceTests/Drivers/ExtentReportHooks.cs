using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using Reqnroll;

namespace Clinica.AcceptanceTests.Drivers;

[Binding]
public class ExtentReportHooks
{
    private static ExtentReports? _extent;
    private static ExtentTest? _feature;
    private static ExtentTest? _scenario;

    private readonly ScenarioContext _scenarioContext;
    private readonly FeatureContext _featureContext;

    public ExtentReportHooks(ScenarioContext scenarioContext, FeatureContext featureContext)
    {
        _scenarioContext = scenarioContext;
        _featureContext = featureContext;
    }

    [BeforeTestRun]
    public static void InitializeReport()
    {
        string projectFolder = @"F:\proyectC#\pruebasUnit\ClinicaObstPrUnit\Clinica.AcceptanceTests";
        string targetFolder = Path.Combine(projectFolder, "TestResults");
        string reportPath = Path.Combine(targetFolder, "ExtentReport.html");
    
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }
    
        var sparkReporter = new ExtentSparkReporter(reportPath);
        sparkReporter.Config.DocumentTitle = "Reporte de Pruebas de Aceptación | SIGEC";
        sparkReporter.Config.ReportName = "Control de Calidad - Clínica Santa Mónica";
        sparkReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

        _extent = new ExtentReports();
        _extent.AttachReporter(sparkReporter);
    
        _extent.AddSystemInfo("Entorno", "Desarrollo Local");
        _extent.AddSystemInfo("Framework", ".NET 9.0");
        _extent.AddSystemInfo("Tecnología Front", "Blazor WASM + MudBlazor");
    }

    [BeforeFeature]
    public static void BeforeFeature(FeatureContext featureContext)
    {
        _feature = _extent?.CreateTest<Feature>(featureContext.FeatureInfo.Title);
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _scenario = _feature?.CreateNode<Scenario>(_scenarioContext.ScenarioInfo.Title);
    }

    [AfterStep]
    public void AfterStep()
    {
        var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
        var stepName = _scenarioContext.StepContext.StepInfo.Text;

        // Determinamos el tipo de paso Gherkin de manera exacta para ExtentReports
        var keyword = stepType switch
        {
            "Given" => _scenario?.CreateNode<Given>(stepName),
            "When" => _scenario?.CreateNode<When>(stepName),
            "Then" => _scenario?.CreateNode<Then>(stepName),
            _ => _scenario?.CreateNode<And>(stepName)
        };

        // Si el paso falló, le inyectamos el color rojo y el mensaje de error en el reporte
        if (_scenarioContext.TestError != null)
        {
            keyword?.Fail(_scenarioContext.TestError.Message);
        }
    }

    [AfterTestRun]
    public static void FlushReport()
    {
        // Un solo método encargado de plasmar el reporte final en el disco duro real
        if (_extent is not null)
        {
            _extent.Flush();
            Console.WriteLine("[INFO] Reporte ExtentReport.html generado exitosamente.");
        }
    }
}