using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Atenciones;

public class AnamnesisDto
{
    [Required, StringLength(1500)] public string MotivoConsulta { get; set; } = string.Empty;
    public int Gestaciones { get; set; }
    public int HijosVivos { get; set; }
    public int Abortos { get; set; }
    public int PartosPretermino { get; set; }
    public int PartosATermino { get; set; }
    public DateTime? FechaUltimaRegla { get; set; }
    public DateTime? FechaProbableParto { get; set; }
    public string? EdadGestacional { get; set; }
    public string? Alergias { get; set; }
    public string? EnfermedadesCronicas { get; set; }
    public string? CirugiasPrevias { get; set; }
    public string? AntecedentesAdicionales { get; set; }
}