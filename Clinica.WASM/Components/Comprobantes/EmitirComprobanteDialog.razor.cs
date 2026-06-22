using Clinica.WASM.Services.Api;
using Clinica.WASM.DTOs.Comprobantes;
using Clinica.WASM.DTOs.Pacientes;
using Clinica.WASM.DTOs.Pagos;
using Clinica.WASM.DTOs.Citas;
using Clinica.WASM.DTOs.Atenciones; // Inyectamos tus DTOs de atención
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Comprobantes;

public partial class EmitirComprobanteDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private ComprobanteApiService ComprobanteService { get; set; } = default!;
    [Inject] private PacienteApiService PacienteService { get; set; } = default!;
    [Inject] private PagoApiService PagoService { get; set; } = default!;
    [Inject] private CitaApiService CitaService { get; set; } = default!;
    [Inject] private AtencionApiService AtencionService { get; set; } = default!; // ◄── Inyección Nueva
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private enum EmissionStep { Input, Preview }
    private EmissionStep CurrentStep = EmissionStep.Input;

    private TipoComprobante TipoSeleccionado = TipoComprobante.BoletaPago;
    private bool IsLoading;

    private PacienteResponseDto? PacienteSeleccionado;
    private PagoResponseDto? PagoSeleccionado;
    private CitaResponseDto? CitaSeleccionada;
    private AtencionResponseDto? AtencionSeleccionada; // ◄── Variable Nueva

    private string AtencionIdInput = string.Empty;

    private List<PacienteResponseDto> todosPacientes = new();
    private List<PagoResponseDto> pagosPaciente = new();
    private List<CitaResponseDto> citasPaciente = new();
    private List<AtencionResponseDto> atencionesPaciente = new(); // ◄── Lista Nueva

    private ComprobantePagoPreviewDto? PreviewBoleta;
    private ComprobanteCitaPreviewDto? PreviewConstancia;
    private ComprobanteAtencionPreviewDto? PreviewAtencion;
    private ComprobanteEstadoCuentaPreviewDto? PreviewEstadoCuenta;

    protected override async Task OnInitializedAsync()
    {
        todosPacientes = await PacienteService.ObtenerTodosAsync();
    }

    private async Task<IEnumerable<PacienteResponseDto>> BuscarPacientesAsync(
        string searchText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
            return todosPacientes.Take(20);

        var texto = searchText.ToLowerInvariant();
        return todosPacientes
            .Where(p => (p.Nombres + " " + p.Apellidos).ToLowerInvariant().Contains(texto)
                        || p.DNI.Contains(texto)
                        || p.CodigoPaciente?.ToLowerInvariant().Contains(texto) == true)
            .Take(20);
    }

    private async Task<IEnumerable<PagoResponseDto>> BuscarPagosAsync(
        string searchText, CancellationToken cancellationToken = default)
    {
        if (PacienteSeleccionado == null)
            return Enumerable.Empty<PagoResponseDto>();

        if (!pagosPaciente.Any())
            pagosPaciente = await PagoService.ObtenerPorPacienteAsync(PacienteSeleccionado.Id);

        if (string.IsNullOrWhiteSpace(searchText))
            return pagosPaciente;

        var texto = searchText.ToLowerInvariant();
        return pagosPaciente
            .Where(p => p.CodigoPago.ToLowerInvariant().Contains(texto)
                        || p.ServicioNombre.ToLowerInvariant().Contains(texto))
            .Take(20);
    }

    private async Task<IEnumerable<CitaResponseDto>> BuscarCitasAsync(
        string searchText, CancellationToken cancellationToken = default)
    {
        if (PacienteSeleccionado == null)
            return Enumerable.Empty<CitaResponseDto>();

        if (!citasPaciente.Any())
            citasPaciente = await CitaService.ObtenerPorPacienteAsync(PacienteSeleccionado.Id);

        if (string.IsNullOrWhiteSpace(searchText))
            return citasPaciente;

        var texto = searchText.ToLowerInvariant();
        return citasPaciente
            .Where(c => c.CodigoCita.ToLowerInvariant().Contains(texto)
                        || c.ServicioNombre.ToLowerInvariant().Contains(texto))
            .Take(20);
    }

    // ✅ NUEVO: Búsqueda Inteligente de Atenciones por Paciente
    private async Task<IEnumerable<AtencionResponseDto>> BuscarAtencionesAsync(
        string searchText, CancellationToken cancellationToken = default)
    {
        if (PacienteSeleccionado == null)
            return Enumerable.Empty<AtencionResponseDto>();

        if (!atencionesPaciente.Any())
            atencionesPaciente = await AtencionService.ObtenerPorPacienteAsync(PacienteSeleccionado.Id);

        if (string.IsNullOrWhiteSpace(searchText))
            return atencionesPaciente;

        var texto = searchText.ToLowerInvariant();
        // Filtra por los primeros caracteres del GUID o propiedades internas de la atención
        return atencionesPaciente
            .Where(a => a.Id.ToString().ToLowerInvariant().Contains(texto))
            .Take(20);
    }

    private async Task LimpiarSelecciones()
    {
        PacienteSeleccionado = null;
        PagoSeleccionado = null;
        CitaSeleccionada = null;
        AtencionSeleccionada = null; // ◄── Limpieza Nueva
        pagosPaciente.Clear();
        citasPaciente.Clear();
        atencionesPaciente.Clear(); // ◄── Limpieza Nueva
        AtencionIdInput = string.Empty;
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        _ = LimpiarSelecciones();
    }

    private void Cancelar() => MudDialog.Cancel();

    private async Task IrAPreview()
    {
        // Actualizamos la validación para usar el objeto de atención seleccionado en vez del string plano
        bool valido = TipoSeleccionado switch
        {
            TipoComprobante.BoletaPago => PagoSeleccionado != null,
            TipoComprobante.ConstanciaCita => CitaSeleccionada != null,
            TipoComprobante.ResumenAtencion => AtencionSeleccionada != null, // ◄── Validación Modificada
            TipoComprobante.EstadoCuenta => PacienteSeleccionado != null,
            _ => false
        };

        if (!valido)
        {
            Snackbar.Add("Complete los datos requeridos.", Severity.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            switch (TipoSeleccionado)
            {
                case TipoComprobante.BoletaPago:
                    PreviewBoleta = await ComprobanteService.PreviewBoletaPagoAsync(PagoSeleccionado!.Id);
                    break;
                case TipoComprobante.ConstanciaCita:
                    PreviewConstancia = await ComprobanteService.PreviewConstanciaCitaAsync(CitaSeleccionada!.Id);
                    break;
                case TipoComprobante.ResumenAtencion:
                    // Enviamos el Id real del Autocomplete
                    PreviewAtencion = await ComprobanteService.PreviewResumenAtencionAsync(AtencionSeleccionada!.Id); // ◄── Modificado
                    break;
                case TipoComprobante.EstadoCuenta:
                    PreviewEstadoCuenta = await ComprobanteService.PreviewEstadoCuentaPacienteAsync(PacienteSeleccionado!.Id);
                    break;
            }
            CurrentStep = EmissionStep.Preview;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al generar vista previa: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EmitirComprobante()
    {
        IsLoading = true;
        try
        {
            bool exitoso = false;
            string mensaje = "";
            Guid? comprobanteId = null;

            switch (TipoSeleccionado)
            {
                case TipoComprobante.BoletaPago:
                    var dtoPago = new EmitirComprobantePagoDto { PagoId = PagoSeleccionado!.Id };
                    (exitoso, mensaje, comprobanteId) = await ComprobanteService.EmitirBoletaPagoAsync(dtoPago);
                    break;
                case TipoComprobante.ConstanciaCita:
                    var dtoCita = new EmitirComprobanteCitaDto { CitaId = CitaSeleccionada!.Id };
                    (exitoso, mensaje, comprobanteId) = await ComprobanteService.EmitirConstanciaCitaAsync(dtoCita);
                    break;
                case TipoComprobante.ResumenAtencion:
                    // Usamos el Id mapeado de la selección limpia
                    var dtoAtencion = new EmitirComprobanteAtencionDto { AtencionId = AtencionSeleccionada!.Id }; // ◄── Modificado
                    (exitoso, mensaje, comprobanteId) = await ComprobanteService.EmitirResumenAtencionAsync(dtoAtencion);
                    break;
                case TipoComprobante.EstadoCuenta:
                    var dtoCuenta = new EmitirComprobanteEstadoCuentaDto { PacienteId = PacienteSeleccionado!.Id };
                    (exitoso, mensaje, comprobanteId) = await ComprobanteService.EmitirEstadoCuentaAsync(dtoCuenta);
                    break;
            }

            if (exitoso)
                MudDialog.Close(DialogResult.Ok(comprobanteId));
            else
                Snackbar.Add(mensaje, Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al emitir comprobante: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void VolverAInput()
    {
        CurrentStep = EmissionStep.Input;
        PreviewBoleta = null;
        PreviewConstancia = null;
        PreviewAtencion = null;
        PreviewEstadoCuenta = null;
    }
}