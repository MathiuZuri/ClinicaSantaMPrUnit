using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Comprobantes;

public class EmitirComprobanteAtencionDto
{
    [Required]
    public Guid AtencionId { get; set; }

    public int TipoComprobante { get; set; } = 3; // ResumenAtencion
    public int FormatoImpresion { get; set; } = 1; // A4

    [StringLength(500)]
    public string? Observacion { get; set; }
}