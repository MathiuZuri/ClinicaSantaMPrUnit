using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Comprobantes;

public class EmitirComprobanteEstadoCuentaDto
{
    [Required]
    public Guid PacienteId { get; set; }

    public int TipoComprobante { get; set; } = 4; // EstadoCuenta
    public int FormatoImpresion { get; set; } = 1; // A4

    [StringLength(500)]
    public string? Observacion { get; set; }
}