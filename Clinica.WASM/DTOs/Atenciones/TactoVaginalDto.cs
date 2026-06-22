using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Atenciones;

public class TactoVaginalDto
{
    public DateTime? FechaHora { get; set; } = DateTime.UtcNow;
    public int? Dilatacion { get; set; }
    public int? Borramiento { get; set; }
    [StringLength(50)] public string? AlturaPresentacion { get; set; }
    [StringLength(100)] public string? MembranasOvulares { get; set; }
    [StringLength(50)] public string? ColorLiquido { get; set; }
    [StringLength(50)] public string? Pelvis { get; set; }
    [StringLength(100)] public string? VariedadPresentacion { get; set; }
}