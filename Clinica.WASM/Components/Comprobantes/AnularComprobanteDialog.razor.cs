using Clinica.WASM.DTOs.Comprobantes;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Comprobantes;

public partial class AnularComprobanteDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private ComprobanteApiService ComprobanteService { get; set; } = default!;
    [Parameter] public Guid ComprobanteId { get; set; }

    private AnularComprobanteDto MotivoModel = new();
    private bool IsLoading;

    private void Cancelar() => MudDialog.Cancel();

    private async Task AnularAsync()
    {
        IsLoading = true;
        var (exitoso, mensaje) = await ComprobanteService.AnularComprobanteAsync(ComprobanteId, MotivoModel.Motivo);
        IsLoading = false;

        if (exitoso)
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            Snackbar.Add(mensaje, Severity.Error, options => options.VisibleStateDuration = 5000);
        }
    }

    [Inject] private ISnackbar Snackbar { get; set; } = default!;
}