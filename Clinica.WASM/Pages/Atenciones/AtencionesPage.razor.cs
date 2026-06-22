using Clinica.WASM.Services.Api;
using Clinica.WASM.DTOs.Atenciones;
using Clinica.WASM.Components.Atenciones;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Atenciones;

public partial class AtencionesPage : ComponentBase
{
    [Inject] private AtencionApiService AtencionService { get; set; } = default!;
    [Inject] private AuthStateService AuthState { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private MudTable<AtencionResponseDto>? Tabla;
    private EstadoAtencion? FiltroEstado;

    private string? ErrorMessage => AtencionService.ErrorMessage;

    protected override async Task OnInitializedAsync()
    {
        var autenticado = await AuthState.EstaAutenticadoAsync();  
        if (!autenticado)
        {
            return;
        }
    }
    
    private async Task OnFiltroEstadoChangedAsync(EstadoAtencion? nuevoEstado)
    {
        if (FiltroEstado == nuevoEstado) return;
        FiltroEstado = nuevoEstado;
        await RecargarTabla();
    }

    private async Task<TableData<AtencionResponseDto>> CargarDatosTabla(TableState state, CancellationToken cancellationToken)
    {
        var atenciones = await AtencionService.ObtenerTodasAsync();
        var filtradas = atenciones
            .Where(a => FiltroEstado == null || a.Estado == FiltroEstado)
            .ToList();

        return new TableData<AtencionResponseDto>
        {
            Items = filtradas.Skip(state.Page * state.PageSize).Take(state.PageSize).ToList(),
            TotalItems = filtradas.Count
        };
    }

    private async Task AbrirRegistro()
    {
        var dialog = await DialogService.ShowAsync<RegistrarAtencionDialog>("Registrar Atención", new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true });
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            Snackbar.Add("Atención registrada exitosamente.", Severity.Success);
            await RecargarTabla();
        }
    }

    private async Task VerDetalle(AtencionResponseDto atencion)
    {
        var parameters = new DialogParameters { ["Atencion"] = atencion };
        await DialogService.ShowAsync<DetalleAtencionDialog>("Detalle de Atención", parameters, new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true });
    }

    private async Task CerrarAtencion(AtencionResponseDto atencion)
    {
        var parameters = new DialogParameters { ["AtencionId"] = atencion.Id };
        var dialog = await DialogService.ShowAsync<CerrarAtencionDialog>("Cerrar Atención", parameters);
        var result = await dialog.Result;
        if (!result.Canceled) { Snackbar.Add("Atención cerrada.", Severity.Success); await RecargarTabla(); }
    }

    private async Task AnularAtencion(AtencionResponseDto atencion)
    {
        var parameters = new DialogParameters { ["AtencionId"] = atencion.Id };
        var dialog = await DialogService.ShowAsync<AnularAtencionDialog>("Anular Atención", parameters);
        var result = await dialog.Result;
        if (!result.Canceled) { Snackbar.Add("Atención anulada.", Severity.Success); await RecargarTabla(); }
    }

    private async Task RecargarTabla()
    {
        if (Tabla != null) await Tabla.ReloadServerData();
    }

    private static Color ObtenerColorEstado(EstadoAtencion estado) => estado switch
    {
        EstadoAtencion.Abierta => Color.Success,
        EstadoAtencion.Cerrada => Color.Info,
        EstadoAtencion.Anulada => Color.Error,
        _ => Color.Default
    };
}