using Clinica.WASM.DTOs.Comprobantes;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace Clinica.WASM.Components.Comprobantes;

public partial class ComprobantesTable : ComponentBase
{
    [Parameter] public TipoComprobante Tipo { get; set; }
    [Parameter] public List<ComprobanteDto> Comprobantes { get; set; } = new();
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback<ComprobanteDto> OnDetalle { get; set; }
    [Parameter] public EventCallback<ComprobanteDto> OnPdf { get; set; }
    [Parameter] public EventCallback<ComprobanteDto> OnAnular { get; set; }

    private List<ComprobanteDto> ComprobantesFiltrados =>
        Comprobantes
            .Where(c => c.TipoComprobante == Tipo.ToString())
            .ToList();
}