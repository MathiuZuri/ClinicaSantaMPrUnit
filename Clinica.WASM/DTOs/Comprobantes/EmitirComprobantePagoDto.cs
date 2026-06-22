using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Comprobantes;

public class EmitirComprobantePagoDto
{
    public Guid PagoId { get; set; }

    [StringLength(50)]
    public string? CodigoPago { get; set; }

    public int TipoComprobante { get; set; } = 1; // BoletaPago
    public int FormatoImpresion { get; set; } = 1; // A4

    [StringLength(500)]
    public string? Observacion { get; set; }
}