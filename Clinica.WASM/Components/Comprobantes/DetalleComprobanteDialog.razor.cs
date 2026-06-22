using Clinica.WASM.DTOs.Comprobantes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Comprobantes;

public partial class DetalleComprobanteDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public ComprobanteDto Comprobante { get; set; } = new();

    private void Cerrar() => MudDialog.Close();
}