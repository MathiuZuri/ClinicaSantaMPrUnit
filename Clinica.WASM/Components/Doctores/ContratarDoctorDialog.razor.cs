using Clinica.WASM.Services.Api;
using Clinica.WASM.DTOs.Doctores;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Doctores;

public partial class ContratarDoctorDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private DoctorApiService DoctorApi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private ContratarDoctorDto Model = new();
    private MudStepper? Stepper;
    private bool IsLoading;

    private void Cancelar() => MudDialog.Cancel();

    private async Task ContratarAsync()
    {
        IsLoading = true;
        var (exitoso, mensaje) = await DoctorApi.ContratarAsync(Model);
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