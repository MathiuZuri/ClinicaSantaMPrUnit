using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Comprobantes;

public class EmitirComprobanteCitaDto
{
    [Required]
    public Guid CitaId { get; set; }

    public int TipoComprobante { get; set; } = 2; // ConstanciaCita
    public int FormatoImpresion { get; set; } = 1; // A4

    [StringLength(500)]
    public string? Observacion { get; set; }
}