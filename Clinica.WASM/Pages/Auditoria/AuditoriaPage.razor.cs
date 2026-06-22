using Clinica.WASM.Services.Api;
using Clinica.WASM.DTOs.Auditoria;
using Clinica.WASM.DTOs.Common;
using Clinica.Domain.Enums;
using Clinica.WASM.Components.Auditoria;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Auditoria;

public partial class AuditoriaPage : ComponentBase
{
    [Inject] private AuditoriaApiService AuditoriaService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    protected MudTable<AuditoriaResponseDto>? Tabla;

    // Propiedad que recarga la tabla al cambiar
    private TipoAccionAuditoria? _filtroTipoAccion;
    protected TipoAccionAuditoria? FiltroTipoAccion
    {
        get => _filtroTipoAccion;
        set
        {
            if (_filtroTipoAccion == value) return;
            _filtroTipoAccion = value;
            Console.WriteLine($"Filtro acción cambiado a: {value}");
            _ = RecargarTabla();
        }
    }

    protected bool FiltroSoloConsultas;

    protected string? ErrorMessage => AuditoriaService.ErrorMessage;

    protected async Task<TableData<AuditoriaResponseDto>> CargarDatosTabla(TableState state, CancellationToken cancellationToken)
    {
        var request = new PaginacionRequestDto
        {
            Pagina = state.Page + 1,
            CantidadPorPagina = state.PageSize
        };

        var resultado = await AuditoriaService.ObtenerTodosAsync(request, FiltroTipoAccion,
            FiltroSoloConsultas ? true : null);

        return new TableData<AuditoriaResponseDto>
        {
            Items = resultado.Datos,
            TotalItems = resultado.TotalRegistros
        };
    }

    private async Task RecargarTabla()
    {
        if (Tabla != null)
        {
            await Tabla.ReloadServerData();
            StateHasChanged();
        }
    }

    protected async Task OnSoloConsultasChangedAsync(bool nuevoValor)
    {
        Console.WriteLine($"Switch cambiado: {nuevoValor}");
        if (FiltroSoloConsultas == nuevoValor) return;
        FiltroSoloConsultas = nuevoValor;
        await RecargarTabla();
    }

    protected async void VerDetalle(AuditoriaResponseDto registro)
    {
        var parameters = new DialogParameters { ["Registro"] = registro };
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        await DialogService.ShowAsync<AuditoriaDetalleDialog>("Detalle de Auditoría", parameters, options);
    }

    protected static Color ObtenerColorTipoAccion(TipoAccionAuditoria tipo)
    {
        return tipo switch
        {
            TipoAccionAuditoria.Creacion => Color.Success,
            TipoAccionAuditoria.Edicion => Color.Warning,
            TipoAccionAuditoria.Eliminacion => Color.Error,
            TipoAccionAuditoria.Login => Color.Info,
            TipoAccionAuditoria.Asignacion => Color.Secondary,
            TipoAccionAuditoria.Error => Color.Dark,
            _ => Color.Default
        };
    }
}