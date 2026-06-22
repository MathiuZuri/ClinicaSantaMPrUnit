using Clinica.WASM.Services.Api;
using Clinica.WASM.DTOs.Atenciones;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Atenciones;

public partial class CerrarAtencionDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private AtencionApiService AtencionApi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [Parameter] public Guid AtencionId { get; set; }

    private CerrarAtencionDto Model = new();
    private bool IsLoading;

    private void Cancelar() => MudDialog.Cancel();

    private async Task CerrarAsync()
    {
        IsLoading = true;
        var (exitoso, mensaje) = await AtencionApi.CerrarAsync(AtencionId, Model);
        IsLoading = false;
        if (exitoso) MudDialog.Close(DialogResult.Ok(true));
        else Snackbar.Add(mensaje, Severity.Error);
    }
}