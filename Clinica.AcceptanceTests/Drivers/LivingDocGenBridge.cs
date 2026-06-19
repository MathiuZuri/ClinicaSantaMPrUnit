using LivingDocGen.Reqnroll.Integration.Bootstrap;
using Reqnroll;

namespace Clinica.AcceptanceTests.Drivers;

[Binding]
public class LivingDocGenBridge
{
    [BeforeTestRun(Order = int.MinValue)]
    public static void BeforeAllTests()
    {
        // Despierta al generador antes de que arranquen las pruebas
        LivingDocBootstrap.BeforeTestRun();
    }

    [AfterTestRun(Order = int.MaxValue)]
    public static void AfterAllTests()
    {
        // Ordena compilar el HTML en el último milisegundo de la ejecución
        LivingDocBootstrap.AfterTestRun();
    }
}