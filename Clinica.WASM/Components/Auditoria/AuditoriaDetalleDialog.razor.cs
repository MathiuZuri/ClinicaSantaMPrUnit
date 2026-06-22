using Clinica.WASM.DTOs.Auditoria;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Auditoria;

public partial class AuditoriaDetalleDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public AuditoriaResponseDto Registro { get; set; } = new();

    private void Cerrar() => MudDialog.Close();
}