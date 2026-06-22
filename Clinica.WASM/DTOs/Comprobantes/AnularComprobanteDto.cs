using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Comprobantes;

public class AnularComprobanteDto
{
    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [MinLength(3, ErrorMessage = "El motivo debe tener al menos 3 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}