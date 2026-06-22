using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Atenciones;

public class ImpresionDiagnosticaDto
{
    [Required, StringLength(500)] public string DiagnosticoPrincipal { get; set; } = string.Empty;
    [StringLength(1000)] public string? DiagnosticosSecundarios { get; set; }
    [StringLength(2500)] public string IndicacionesReceta { get; set; } = string.Empty;
    public DateTime? FechaProximaCita { get; set; }
    [StringLength(250)] public string? MotivoProximaCita { get; set; }
}