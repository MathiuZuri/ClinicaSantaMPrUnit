using Clinica.WASM.DTOs.Atenciones;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Atenciones;

public partial class AnularAtencionDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private AtencionApiService AtencionApi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [Parameter] public Guid AtencionId { get; set; }

    private AnularAtencionDto MotivoModel = new();
    private bool IsLoading;

    private void Cancelar() => MudDialog.Cancel();

    private async Task AnularAsync()
    {
        IsLoading = true;
        var (exitoso, mensaje) = await AtencionApi.AnularAsync(AtencionId, MotivoModel.Motivo);
        IsLoading = false;
        if (exitoso) MudDialog.Close(DialogResult.Ok(true));
        else Snackbar.Add(mensaje, Severity.Error);
    }
}