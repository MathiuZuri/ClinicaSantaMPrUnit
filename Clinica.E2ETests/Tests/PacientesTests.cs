using Clinica.E2ETests.Helpers;
using Microsoft.Playwright;
using Xunit;

namespace Clinica.E2ETests.Tests;

public class PacientesTests : E2ETestBase
{
    [Fact]
    public async Task CargarPaginaPacientes_DespuesDeLogin_MuestraTitulo()
    {
        await AuthHelper.LoginAsAdminAsync(Page);
        await Page.GotoAsync("/panel/pacientes");
        
        // Esperar a que la grilla de control de pacientes sea visible en el DOM
        await Page.WaitForSelectorAsync(".custom-luxury-table", new() { State = WaitForSelectorState.Visible });

        // Aserción semántica del encabezado del módulo
        var headerTitle = Page.Locator(".header-title-text, h1, h2, h3").Filter(new() { HasText = "Módulo de Pacientes" }).First;
        await Assertions.Expect(headerTitle).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CrearPaciente_Valido_DeberiaAparecerEnTabla()
    {
        await AuthHelper.LoginAsAdminAsync(Page);
        await Page.GotoAsync("/panel/pacientes");
        await Page.WaitForSelectorAsync(".custom-luxury-table", new() { State = WaitForSelectorState.Visible });

        await Page.Locator("button:has-text('Nuevo paciente')").First.ClickAsync();
        await Page.WaitForSelectorAsync("text=Registrar Nuevo Expediente Médico", new() { State = WaitForSelectorState.Visible });

        var dni = Random.Shared.Next(10000000, 99999999).ToString();
        await Page.GetByLabel("Número de DNI").FillAsync(dni);
        await Page.GetByLabel("Nombres de la Paciente").FillAsync("María");
        await Page.GetByLabel("Apellidos Completos").FillAsync("González López");

        // Interacción segura con el DatePicker
        var fechaPicker = Page.GetByLabel("Fecha de Nacimiento");
        await fechaPicker.ClickAsync();
        await Page.Locator(".mud-popover-open .mud-day:has-text('10')").First.ClickAsync();

        // Selección de Sexo Biológico en el menú desplegable flotante
        var sexoSelect = Page.GetByLabel("Sexo Biológico");
        await sexoSelect.ClickAsync();
        await Page.WaitForSelectorAsync(".mud-popover-open .mud-list-item", new() { State = WaitForSelectorState.Visible });
        await Page.Locator(".mud-popover-open .mud-list-item").Filter(new() { HasText = "Femenino" }).ClickAsync();

        await Page.GetByLabel("Número Celular").FillAsync("987654321");
        await Page.GetByLabel("Correo Electrónico Notificable").FillAsync("maria@test.com");
        await Page.GetByLabel("Dirección Domiciliaria Completa").FillAsync("Av. Siempre Viva 742");

        // Envío del formulario
        await Page.Locator("button[type='submit']").ClickAsync();

        // CORRECCIÓN: Validar mediante la alerta del contenedor localizado
        var alerta = Page.GetByRole(AriaRole.Alert);
        await Assertions.Expect(alerta).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Assertions.Expect(alerta).ToContainTextAsync("Paciente registrado");

        // Comprobación de persistencia inmediata en la grilla
        var celdaDni = Page.Locator(".custom-luxury-table tbody").GetByText(dni);
        await Assertions.Expect(celdaDni).ToBeVisibleAsync();
    }

    [Fact]
    public async Task VerDetallePaciente_DeberiaMostrarDialogoConInformacion()
    {
        await CrearPacienteDePruebaSiNoExiste();
        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync(".custom-luxury-table", new() { State = WaitForSelectorState.Visible });

        // CORRECCIÓN: Primer botón de la fila de acciones (Ver Ficha -> Índice 0)
        var btnVer = Page.Locator("td[data-label='Acciones'] button").Nth(0);
        await btnVer.ClickAsync();

        var dialogo = Page.Locator(".mud-dialog");
        await Assertions.Expect(dialogo).ToBeVisibleAsync();
        
        await Assertions.Expect(dialogo.Locator(".patient-dialog-name")).ToBeVisibleAsync();
        await Assertions.Expect(dialogo).ToContainTextAsync("DNI");

        // Cierre del modal de perfil médico
        await Page.Locator(".mud-dialog button:has-text('Cerrar Ficha')").ClickAsync();
        await Assertions.Expect(dialogo).ToBeHiddenAsync();
    }

    [Fact]
    public async Task CambiarEstadoPaciente_DeberiaActualizarEstadoEnTabla()
    {
        await CrearPacienteDePruebaSiNoExiste();
        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync(".custom-luxury-table", new() { State = WaitForSelectorState.Visible });

        // CORRECCIÓN: Tercer botón de la fila de acciones (Cambiar Estado -> Índice 2)
        var btnEstado = Page.Locator("td[data-label='Acciones'] button").Nth(2);
        await btnEstado.ClickAsync();

        var dialogo = Page.Locator(".mud-dialog");
        await Assertions.Expect(dialogo).ToBeVisibleAsync();

        var estadoSelect = Page.GetByLabel("Nuevo Estado Clínico Asignado");
        await estadoSelect.ClickAsync();
        
        await Page.WaitForSelectorAsync(".mud-popover-open .mud-list-item", new() { State = WaitForSelectorState.Visible });
        await Page.Locator(".mud-popover-open .mud-list-item").Filter(new() { HasText = "Inactivo (Falta de atención)" }).ClickAsync();

        await Page.Locator(".mud-dialog button[type='submit']").ClickAsync();

        var alerta = Page.GetByRole(AriaRole.Alert);
        await Assertions.Expect(alerta).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Assertions.Expect(alerta).ToContainTextAsync("Estado del paciente actualizado");
    }

    [Fact]
    public async Task EditarContacto_DeberiaActualizarCelularYCorreo()
    {
        await CrearPacienteDePruebaSiNoExiste();
        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync(".custom-luxury-table", new() { State = WaitForSelectorState.Visible });

        // CORRECCIÓN: Segundo botón de la fila de acciones (Actualizar Contacto -> Índice 1)
        var btnContacto = Page.Locator("td[data-label='Acciones'] button").Nth(1);
        await btnContacto.ClickAsync();

        // Validar transición de la vista inline (no es un modal)
        await Page.WaitForSelectorAsync("text=Modificar Datos de Contacto Externo", new() { State = WaitForSelectorState.Visible });

        await Page.GetByLabel("Teléfono Celular").FillAsync("999888777");
        await Page.GetByLabel("Correo Electrónico").FillAsync("actualizado@test.com");

        await Page.Locator("button:has-text('Actualizar contacto')").ClickAsync();

        var alerta = Page.GetByRole(AriaRole.Alert);
        await Assertions.Expect(alerta).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Assertions.Expect(alerta).ToContainTextAsync("Contacto del paciente actualizado");
    }

    private async Task CrearPacienteDePruebaSiNoExiste()
    {
        await AuthHelper.LoginAsAdminAsync(Page);
        await Page.GotoAsync("/panel/pacientes");
        await Page.WaitForSelectorAsync(".custom-luxury-table", new() { State = WaitForSelectorState.Visible });

        var totalFilas = await Page.Locator(".custom-luxury-table tbody tr").CountAsync();
        if (totalFilas == 0 || await Page.Locator("text=No hay pacientes registrados").CountAsync() > 0)
        {
            await Page.Locator("button:has-text('Nuevo paciente')").First.ClickAsync();
            await Page.WaitForSelectorAsync("text=Registrar Nuevo Expediente Médico", new() { State = WaitForSelectorState.Visible });

            var dni = Random.Shared.Next(10000000, 99999999).ToString();
            await Page.GetByLabel("Número de DNI").FillAsync(dni);
            await Page.GetByLabel("Nombres de la Paciente").FillAsync("Test");
            await Page.GetByLabel("Apellidos Completos").FillAsync("Automatizado");

            var fechaPicker = Page.GetByLabel("Fecha de Nacimiento");
            await fechaPicker.ClickAsync();
            await Page.Locator(".mud-popover-open .mud-day:has-text('15')").First.ClickAsync();

            await Page.GetByLabel("Sexo Biológico").ClickAsync();
            await Page.Locator(".mud-popover-open .mud-list-item").Filter(new() { HasText = "Femenino" }).ClickAsync();

            await Page.GetByLabel("Número Celular").FillAsync("987654321");
            await Page.GetByLabel("Correo Electrónico Notificable").FillAsync("test@test.com");
            await Page.GetByLabel("Dirección Domiciliaria Completa").FillAsync("Calle Test");

            await Page.Locator("button[type='submit']").ClickAsync();
            await Page.WaitForSelectorAsync("[role='alert']", new() { State = WaitForSelectorState.Visible });
        }
    }
}