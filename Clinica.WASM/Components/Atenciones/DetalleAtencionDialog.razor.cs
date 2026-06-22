using Clinica.WASM.DTOs.Atenciones;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Atenciones;

public partial class DetalleAtencionDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public AtencionResponseDto Atencion { get; set; } = new();

    private void Cerrar() => MudDialog.Close();
    
}