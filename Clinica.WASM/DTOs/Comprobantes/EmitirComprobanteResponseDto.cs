namespace Clinica.WASM.DTOs.Comprobantes;

public class EmitirComprobanteResponseDto
{
    public string Mensaje { get; set; } = string.Empty;
    public Guid ComprobanteId { get; set; }
}