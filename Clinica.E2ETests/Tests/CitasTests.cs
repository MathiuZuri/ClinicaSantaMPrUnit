using Clinica.E2ETests.Helpers;
using Microsoft.Playwright;
using Xunit;

namespace Clinica.E2ETests.Tests;

public class CitasTests : E2ETestBase
{
    [Fact]
    public async Task CargarPaginaCitas_DespuesDeLogin_MuestraTitulo()
    {
        await AuthHelper.LoginAsAdminAsync(Page);
        await Page.GotoAsync("/panel/citas");

        // Esperar que la grilla de datos de la clínica sea visible
        await Page.WaitForSelectorAsync(".luxury-styled-table", new() { State = WaitForSelectorState.Visible });

        var headerTitle = Page.Locator(".master-card-title, h1, h3").Filter(new() { HasText = "Agenda de Citas Médicas" });
        await Assertions.Expect(headerTitle).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CrearCita_Valida_DeberiaAparecerEnTabla()
    {
        await AuthHelper.LoginAsAdminAsync(Page);
        await Page.GotoAsync("/panel/citas");
        await Page.WaitForSelectorAsync(".luxury-styled-table", new() { State = WaitForSelectorState.Visible });

        await Page.Locator("button:has-text('Nueva cita')").First.ClickAsync();
        await Page.WaitForSelectorAsync("text=Programar Nueva Consulta Médica", new() { State = WaitForSelectorState.Visible });

        // Autocomplete de Paciente
        var pacienteInput = Page.GetByLabel("Paciente Asignado");
        await pacienteInput.ClickAsync();
        await pacienteInput.FillAsync("a");
        await Page.WaitForSelectorAsync(".mud-popover-open .mud-list-item", new() { State = WaitForSelectorState.Visible });
        await Page.Locator(".mud-popover-open .mud-list-item").First.ClickAsync();

        // Selector de Doctor
        var doctorSelect = Page.GetByLabel("Especialista Ginecobstetra");
        await doctorSelect.ClickAsync();
        await Page.WaitForSelectorAsync(".mud-popover-open .mud-list-item", new() { State = WaitForSelectorState.Visible });
        await Page.Locator(".mud-popover-open .mud-list-item").First.ClickAsync();

        // Selector de Servicio
        var servicioSelect = Page.GetByLabel("Servicio Clínico / Módulo");
        await servicioSelect.ClickAsync();
        await Page.WaitForSelectorAsync(".mud-popover-open .mud-list-item", new() { State = WaitForSelectorState.Visible });
        await Page.Locator(".mud-popover-open .mud-list-item").First.ClickAsync();

        await Page.GetByLabel("Motivo Clínico de la Consulta").FillAsync("Control prenatal de rutina");

        // Enviar el formulario usando el botón específico de la barra de acciones
        await Page.Locator(".form-actions-bar button[type='submit']").ClickAsync();

        // AJUSTE: Validar usando el rol de alerta global (Snackbar de MudBlazor)
        var snackbar = Page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "Cita programada" });
        await Assertions.Expect(snackbar).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Fact]
    public async Task VerDetalleCita_DeberiaMostrarDialogoConInformacion()
    {
        await AsegurarCitaDePruebaAsync();
        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync(".luxury-styled-table", new() { State = WaitForSelectorState.Visible });

        // Click en el primer botón de la fila (Ver Ficha)
        var btnVer = Page.Locator("td[data-label='Acciones'] button").Nth(0);
        await btnVer.ClickAsync();

        var dialogo = Page.Locator(".mud-dialog");
        await Assertions.Expect(dialogo).ToBeVisibleAsync();

        var dialogoTitulo = await Page.Locator(".appointment-dialog-code").TextContentAsync();
        Assert.Contains("Ticket:", dialogoTitulo);

        await Page.Locator(".mud-dialog button:has-text('Cerrar Ficha')").ClickAsync();
        await Assertions.Expect(dialogo).ToBeHiddenAsync();
    }

    [Fact]
    public async Task CancelarCita_DeberiaCambiarEstadoEnTabla()
    {
        await AsegurarCitaDePruebaAsync();
        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync(".luxury-styled-table", new() { State = WaitForSelectorState.Visible });

        // AJUSTE: Click en Anular Ticket (Tercer botón de la celda de acciones -> Índice 2)
        var btnCancelar = Page.Locator("td[data-label='Acciones'] button").Nth(2);
        await btnCancelar.ClickAsync();

        // Validar que el modal de anulación se abrió correctamente
        var dialogo = Page.Locator(".mud-dialog");
        await Assertions.Expect(dialogo).ToBeVisibleAsync();

        // Rellenar el motivo en el input del diálogo
        await Page.GetByLabel("Motivo justificado de la cancelación").FillAsync("Paciente no puede asistir por cruce de horarios");

        // AJUSTE: Hacer clic de forma estricta en el botón de confirmación interno del modal
        await Page.Locator(".mud-dialog button:has-text('Confirmar cancelación')").ClickAsync();

        // AJUSTE: Esperar el snackbar global de confirmación de la API
        var snackbar = Page.GetByRole(AriaRole.Alert);
        await Assertions.Expect(snackbar).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Fact]
    public async Task ReprogramarCita_DeberiaActualizarFechaYHora()
    {
        await AsegurarCitaDePruebaAsync();
        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync(".luxury-styled-table", new() { State = WaitForSelectorState.Visible });

        // Click en Reprogramar Bloque (Segundo botón de la celda de acciones -> Índice 1)
        var btnReprogramar = Page.Locator("td[data-label='Acciones'] button").Nth(1);
        await btnReprogramar.ClickAsync();

        var dialogo = Page.Locator(".mud-dialog");
        await Assertions.Expect(dialogo).ToBeVisibleAsync();

        await Page.GetByLabel("Motivo justificado del cambio de horario").FillAsync("Ajuste de agenda operativa");
        
        // AJUSTE: Click estricto en el botón de confirmación del diálogo
        await Page.Locator(".mud-dialog button:has-text('Reprogramar')").ClickAsync();

        // AJUSTE: Localizar el rol alert global que capturó tu snapshot ("Cita reprogramada.")
        var snackbar = Page.GetByRole(AriaRole.Alert);
        await Assertions.Expect(snackbar).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task AsegurarCitaDePruebaAsync()
    {
        await AuthHelper.LoginAsAdminAsync(Page);
        await Page.GotoAsync("/panel/citas");
        await Page.WaitForSelectorAsync(".luxury-styled-table", new() { State = WaitForSelectorState.Visible });

        var totalFilas = await Page.Locator(".luxury-styled-table tbody tr").CountAsync();
        if (totalFilas == 0 || await Page.Locator("text=No hay citas registradas").CountAsync() > 0)
        {
            await Page.Locator("button:has-text('Nueva cita')").First.ClickAsync();
            
            var pacienteInput = Page.GetByLabel("Paciente Asignado");
            await pacienteInput.ClickAsync();
            await pacienteInput.FillAsync("a");
            await Page.Locator(".mud-popover-open .mud-list-item").First.ClickAsync();

            await Page.GetByLabel("Especialista Ginecobstetra").ClickAsync();
            await Page.Locator(".mud-popover-open .mud-list-item").First.ClickAsync();

            await Page.GetByLabel("Servicio Clínico / Módulo").ClickAsync();
            await Page.Locator(".mud-popover-open .mud-list-item").First.ClickAsync();

            await Page.GetByLabel("Motivo Clínico de la Consulta").FillAsync("Cita semilla automatizada");
            await Page.Locator(".form-actions-bar button[type='submit']").ClickAsync();
            
            await Page.WaitForSelectorAsync("[role='alert']", new() { State = WaitForSelectorState.Visible });
        }
    }
}