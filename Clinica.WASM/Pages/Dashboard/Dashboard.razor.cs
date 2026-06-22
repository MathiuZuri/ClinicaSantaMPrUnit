using Microsoft.AspNetCore.Components;
using Clinica.WASM.Services.Api;

namespace Clinica.WASM.Pages.Dashboard;

public partial class Dashboard : ComponentBase
{
    [Inject] private PacienteApiService PacienteApi { get; set; } = default!;
    [Inject] private CitaApiService CitaApi { get; set; } = default!;
    [Inject] private AtencionApiService AtencionApi { get; set; } = default!;
    [Inject] private FinanzasApiService FinanzasApi { get; set; } = default!;
    [Inject] private DoctorApiService DoctorApi { get; set; } = default!;

    protected int ContadorPacientes { get; set; }
    protected int ContadorCitas { get; set; }
    protected int ContadorAtenciones { get; set; }
    protected decimal IngresosMensuales { get; set; }
    protected int ContadorPagosPendientes { get; set; }
    protected decimal DeudaTotal { get; set; }
    protected int ContadorDoctoresActivos { get; set; }
    protected int ContadorAtencionesHoy { get; set; }
    protected bool CargandoMetricas { get; set; } = true;
    protected bool HuboErrorCarga { get; set; }
    protected string MensajeError { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarMetricasAsync();
    }

    private async Task CargarMetricasAsync()
    {
        CargandoMetricas = true;
        HuboErrorCarga = false;
        try
        {
            // Llamadas en paralelo para optimizar el rendimiento
            var tareas = new Task[]
            {
                CargarPacientesAsync(),
                CargarCitasHoyAsync(),
                CargarAtencionesMesAsync(),
                CargarFinanzasAsync(),
                CargarDoctoresActivosAsync(),
                CargarAtencionesHoyAsync()
            };

            await Task.WhenAll(tareas);
        }
        catch (Exception ex)
        {
            HuboErrorCarga = true;
            MensajeError = $"Error al cargar indicadores: {ex.Message}";
        }
        finally
        {
            CargandoMetricas = false;
        }
    }

    private async Task CargarPacientesAsync()
    {
        var pacientes = await PacienteApi.ObtenerTodosAsync();
        ContadorPacientes = pacientes.Count;
    }

    private async Task CargarCitasHoyAsync()
    {
        var todasCitas = await CitaApi.ObtenerTodasAsync();
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        ContadorCitas = todasCitas.Count(c => c.Fecha == hoy);
    }

    private async Task CargarAtencionesMesAsync()
    {
        var atenciones = await AtencionApi.ObtenerTodasAsync();
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        ContadorAtenciones = atenciones.Count(a => a.FechaInicio >= inicioMes);
    }

    private async Task CargarFinanzasAsync()
    {
        var resumenMensual = await FinanzasApi.ObtenerResumenMensualAsync(DateTime.Today.Year, DateTime.Today.Month);
        IngresosMensuales = resumenMensual?.TotalIngresos ?? 0;
        DeudaTotal = resumenMensual?.TotalDeuda ?? 0;

        var pagosPendientes = await FinanzasApi.ObtenerPagosPendientesAsync();
        ContadorPagosPendientes = pagosPendientes.Count;
    }

    private async Task CargarDoctoresActivosAsync()
    {
        var doctoresActivos = await DoctorApi.ObtenerActivosAsync();
        ContadorDoctoresActivos = doctoresActivos.Count;
    }

    private async Task CargarAtencionesHoyAsync()
    {
        var atenciones = await AtencionApi.ObtenerTodasAsync();
        var hoy = DateTime.Today;
        ContadorAtencionesHoy = atenciones.Count(a => a.FechaInicio.Date == hoy);
    }
}