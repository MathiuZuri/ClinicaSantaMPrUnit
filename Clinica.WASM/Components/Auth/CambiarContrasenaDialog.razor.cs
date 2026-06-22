using Clinica.WASM.DTOs.Auth;
using Clinica.WASM.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Auth;

public partial class CambiarContrasenaDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private AuthApiService AuthApi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private CambiarContrasenaDto Model = new();
    private bool IsLoading;

    private void Cancelar() => MudDialog.Cancel();

    private async Task CambiarAsync()
    {
        IsLoading = true;
        var (exitoso, mensaje) = await AuthApi.CambiarContrasenaAsync(Model);
        IsLoading = false;

        if (exitoso)
        {
            Snackbar.Add(mensaje, Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            Snackbar.Add(mensaje, Severity.Error);
        }
    }
}