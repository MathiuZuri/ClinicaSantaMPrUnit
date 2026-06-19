using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace Clinica.AcceptanceTests.Steps;

[Binding]
public class CitasSteps
{
    private readonly IPage _page;
    private const string BaseUrl = "https://localhost:7299";

    private string? _codigoCitaActual;
    private string _fechaGenerada = string.Empty;
    private string _horaInicioGenerada = string.Empty;
    private string _horaFinGenerada = string.Empty;

    public CitasSteps(IPage page)
    {
        _page = page;
    }

    #region Background

    [Given(@"que el usuario ha iniciado sesión como administrador")]
    public async Task DadoQueElUsuarioHaIniciadoSesionComoAdministrador()
    {
        await _page.GotoAsync($"{BaseUrl}/login");
        await _page.GetByLabel("Usuario o Correo Electrónico").FillAsync("admin");
        await _page.GetByLabel("Contraseña").FillAsync("admin123");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Ingresar a Intranet" }).ClickAsync();
        await _page.WaitForURLAsync($"{BaseUrl}/dashboard");
    }

    [Given(@"navega a la página de citas ""(.*)""")]
    public async Task DadoQueNavegaALaPaginaDeCitas(string ruta)
    {
        await _page.GotoAsync($"{BaseUrl}{ruta}");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Genera una fecha futura dentro del próximo mes (30 días) para evitar rechazos por fecha pasada o muy lejana.
    /// </summary>
    private string GenerarFechaUnica()
    {
        return DateTime.Today.AddDays(30).ToString("dd/MM/yyyy");
    }

    /// <summary>
    /// Devuelve horas fijas en la mañana (08:00 - 08:30) que siempre son futuras si la fecha es futura.
    /// </summary>
    private (string inicio, string fin) ObtenerHorasFijasManana()
    {
        return ("08:00", "08:30");
    }

    private async Task SeleccionarFechaAsync(string label, string fecha)
    {
        var input = _page.GetByLabel(label);
        await input.ClickAsync();

        var partes = fecha.Split('/');
        int dia = int.Parse(partes[0]);

        var diaBoton = _page.GetByRole(AriaRole.Button, new() { Name = dia.ToString(), Exact = true });
        await diaBoton.ClickAsync();
    }

    private async Task SeleccionarHoraAsync(string label, string hora)
    {
        var input = _page.GetByLabel(label);
        await input.ClickAsync();
        await _page.Keyboard.TypeAsync(hora);
        await _page.Keyboard.PressAsync("Enter");
    }

    #endregion

    #region Creación de cita (escenario exitoso)

    [When(@"hace clic en el botón ""(.*)""")]
    public async Task CuandoHaceClicEnElBoton(string nombreBoton)
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = nombreBoton }).ClickAsync();
    }

    [When(@"selecciona el paciente ""(.*)""")]
    public async Task CuandoSeleccionaElPaciente(string nombrePaciente)
    {
        var autocomplete = _page.GetByLabel("Paciente Asignado");
        await autocomplete.FillAsync(nombrePaciente);
        var opcion = _page.Locator("[role=\"option\"]").Filter(new() { HasText = nombrePaciente }).First;
        await opcion.ClickAsync();
    }

    [When(@"selecciona el especialista ""(.*)""")]
    public async Task CuandoSeleccionaElEspecialista(string nombreDoctor)
    {
        await _page.GetByLabel("Especialista Ginecobstetra").ClickAsync();
        var opcion = _page.Locator("[role=\"option\"]").Filter(new() { HasText = nombreDoctor }).First;
        await opcion.ClickAsync();
    }

    [When(@"selecciona el servicio clínico ""(.*)""")]
    public async Task CuandoSeleccionaElServicioClinico(string nombreServicio)
    {
        await _page.GetByLabel("Servicio Clínico / Módulo").ClickAsync();
        var opcion = _page.Locator("[role=\"option\"]").Filter(new() { HasText = nombreServicio }).First;
        await opcion.ClickAsync();
    }

    [When(@"configura una fecha y hora disponibles sin conflictos")]
    public async Task CuandoConfiguraUnaFechaYHoraDisponiblesSinConflictos()
    {
        _fechaGenerada = GenerarFechaUnica();
        var (inicio, fin) = ObtenerHorasFijasManana();
        _horaInicioGenerada = inicio;
        _horaFinGenerada = fin;

        await SeleccionarFechaAsync("Fecha Calendario", _fechaGenerada);
        await SeleccionarHoraAsync("Hora Apertura (Inicio)", _horaInicioGenerada);
        await SeleccionarHoraAsync("Hora Cierre (Fin)", _horaFinGenerada);
    }

    [When(@"escribe en el motivo clínico ""(.*)""")]
    public async Task CuandoEscribeElMotivoClinico(string motivo)
    {
        await _page.GetByLabel("Motivo Clínico de la Consulta").FillAsync(motivo);
    }

    [Then(@"el sistema muestra una alerta de éxito con el mensaje ""(.*)""")]
    public async Task EntoncesElSistemaMuestraUnaAlertaDeExito(string mensajeEsperado)
    {
        var alerta = _page.Locator(".mud-alert-message");
        await alerta.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });
        var texto = await alerta.TextContentAsync();
        Assert.That(texto, Does.Contain(mensajeEsperado));
    }

    [Then(@"la grilla de citas muestra una cita para el paciente ""(.*)"" con estado ""(.*)""")]
    public async Task EntoncesLaGrillaMuestraCitaParaPacienteConEstado(string paciente, string estado)
    {
        var fila = _page.Locator("tr", new() { HasText = paciente });
        await fila.WaitForAsync();
        var celdaEstado = fila.Locator("td").Filter(new() { HasText = estado });
        Assert.That(await celdaEstado.IsVisibleAsync(), Is.True);
    }

    #endregion

    #region Validación de campos vacíos

    [When(@"hace clic directamente en el botón ""(.*)"" sin llenar los campos")]
    public async Task CuandoHaceClicDirectamenteEnElBotonSinLlenarLosCampos(string nombreBoton)
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = nombreBoton }).ClickAsync();
    }

    [Then(@"el formulario muestra errores de validación para los campos requeridos")]
    public async Task EntoncesElFormularioMuestraErroresDeValidacion()
    {
        // Tomar captura para diagnóstico antes de fallar
        try
        {
            var errorElement = _page.Locator(".mud-input-error");
            await errorElement.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
            Assert.That(await errorElement.First.IsVisibleAsync(), Is.True);
        }
        catch (TimeoutException)
        {
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "validacion_error.png" });
            Assert.Fail("No se encontraron mensajes de error de validación tras 10s. Se guardó 'validacion_error.png'.");
        }
    }

    #endregion

    #region Escenarios con cita previa (creación + captura de código)

    [Given(@"que se ha creado una cita de prueba para el paciente ""(.*)""")]
    public async Task DadoQueSeHaCreadoUnaCitaDePrueba(string paciente)
    {
        _fechaGenerada = GenerarFechaUnica();
        var (inicio, fin) = ObtenerHorasFijasManana();
        _horaInicioGenerada = inicio;
        _horaFinGenerada = fin;

        await _page.GetByRole(AriaRole.Button, new() { Name = "Nueva cita" }).ClickAsync();

        await _page.GetByLabel("Paciente Asignado").FillAsync(paciente);
        await _page.Locator("[role=\"option\"]").Filter(new() { HasText = paciente }).First.ClickAsync();

        await _page.GetByLabel("Especialista Ginecobstetra").ClickAsync();
        await _page.Locator("[role=\"option\"]").First.ClickAsync();

        await _page.GetByLabel("Servicio Clínico / Módulo").ClickAsync();
        await _page.Locator("[role=\"option\"]").First.ClickAsync();

        await SeleccionarFechaAsync("Fecha Calendario", _fechaGenerada);
        await SeleccionarHoraAsync("Hora Apertura (Inicio)", _horaInicioGenerada);
        await SeleccionarHoraAsync("Hora Cierre (Fin)", _horaFinGenerada);
        await _page.GetByLabel("Motivo Clínico de la Consulta").FillAsync("Motivo de prueba automatizado");

        await _page.GetByRole(AriaRole.Button, new() { Name = "Programar cita" }).ClickAsync();

        var alerta = _page.Locator(".mud-alert-message");
        await alerta.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });
        var texto = await alerta.TextContentAsync();
        Assert.That(texto, Does.Contain("Cita programada"), $"No se confirmó la creación de la cita de prueba. Alerta: '{texto}'");
    }

    [Given(@"se ha obtenido el código de la cita generada")]
    public async Task DadoSeHaObtenidoElCodigoDeLaCitaGenerada()
    {
        var primerFila = _page.Locator("tbody tr").First;
        var badge = primerFila.Locator(".appointment-code-badge");
        _codigoCitaActual = await badge.TextContentAsync();
        Assert.That(_codigoCitaActual, Is.Not.Null.And.Not.Empty);
    }

    [When(@"hace clic en el botón ""(.*)"" de la cita capturada")]
    public async Task CuandoHaceClicEnElBotonDeLaCitaCapturada(string nombreBoton)
    {
        if (string.IsNullOrEmpty(_codigoCitaActual))
            Assert.Fail("No se ha capturado el código de la cita previamente.");

        var fila = _page.Locator("tr", new() { HasText = _codigoCitaActual });
        await fila.WaitForAsync();

        var boton = fila.GetByTitle(nombreBoton);
        if (await boton.CountAsync() == 0)
        {
            string iconSelector = nombreBoton switch
            {
                "Reprogramar Bloque" => "svg[id*='EditCalendar']",
                "Anular Ticket" => "svg[id*='Cancel']",
                _ => null
            };
            if (iconSelector != null)
            {
                boton = fila.Locator("button").Filter(new() { Has = fila.Locator(iconSelector) });
            }
            else
            {
                Assert.Fail($"No se encontró un selector para el botón '{nombreBoton}'");
            }
        }

        await boton.ClickAsync();
    }

    [When(@"configura la nueva fecha ""(.*)""")]
    public async Task CuandoConfiguraLaNuevaFecha(string fecha)
    {
        await SeleccionarFechaAsync("Nueva fecha asignada", fecha);
    }

    [When(@"configura el nuevo horario de inicio ""(.*)"" y fin ""(.*)""")]
    public async Task CuandoConfiguraElNuevoHorario(string inicio, string fin)
    {
        await SeleccionarHoraAsync("Hora inicio", inicio);
        await SeleccionarHoraAsync("Hora fin", fin);
    }

    [When(@"escribe el motivo de reprogramación ""(.*)""")]
    public async Task CuandoEscribeElMotivoDeReprogramacion(string motivo)
    {
        await _page.GetByLabel("Motivo justificado del cambio de horario").FillAsync(motivo);
    }

    [When(@"escribe el motivo de cancelación ""(.*)""")]
    public async Task CuandoEscribeElMotivoDeCancelacion(string motivo)
    {
        await _page.GetByLabel("Motivo justificado de la cancelación").FillAsync(motivo);
    }

    [Then(@"la grilla de citas muestra la cita capturada con estado ""(.*)"" y el nuevo horario ""(.*)""")]
    public async Task EntoncesLaGrillaMuestraLaCitaConEstadoYHorario(string estado, string horario)
    {
        if (string.IsNullOrEmpty(_codigoCitaActual))
            Assert.Fail("Código de cita no disponible.");

        var fila = _page.Locator("tr", new() { HasText = _codigoCitaActual });
        await fila.WaitForAsync();

        var celdaEstado = fila.Locator("td").Filter(new() { HasText = estado });
        Assert.That(await celdaEstado.IsVisibleAsync(), Is.True);

        var celdaHorario = fila.Locator("td").Filter(new() { HasText = horario });
        Assert.That(await celdaHorario.IsVisibleAsync(), Is.True);
    }

    [Then(@"la grilla de citas muestra la cita capturada con estado ""(.*)"" y sin acciones de reprogramación o anulación")]
    public async Task EntoncesLaGrillaMuestraLaCitaCanceladaSinAcciones(string estado)
    {
        if (string.IsNullOrEmpty(_codigoCitaActual))
            Assert.Fail("Código de cita no disponible.");

        var fila = _page.Locator("tr", new() { HasText = _codigoCitaActual });
        await fila.WaitForAsync();

        var celdaEstado = fila.Locator("td").Filter(new() { HasText = estado });
        Assert.That(await celdaEstado.IsVisibleAsync(), Is.True);

        var btnReprogramar = fila.Locator("button").Filter(new() { Has = fila.Locator("svg[id*='EditCalendar']") });
        var btnAnular = fila.Locator("button").Filter(new() { Has = fila.Locator("svg[id*='Cancel']") });

        Assert.That(await btnReprogramar.IsVisibleAsync(), Is.False, "El botón Reprogramar aún está presente.");
        Assert.That(await btnAnular.IsVisibleAsync(), Is.False, "El botón Anular aún está presente.");
    }

    #endregion
}