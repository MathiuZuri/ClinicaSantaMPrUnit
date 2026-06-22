using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Atenciones;

public class EcografiaObstetricaDto
{
    public DateTime? FechaHora { get; set; } = DateTime.UtcNow;
    public int? DiametroBiparietal { get; set; }
    public int? CircunferenciaCefalica { get; set; }
    public int? CircunferenciaAbdominal { get; set; }
    public int? LongitudFemur { get; set; }
    public int? PesoFetalEstimado { get; set; }
    public decimal? IndiceLiquidoAmniotico { get; set; }
    [StringLength(100)] public string? PlacentaLocalizacion { get; set; }
    [StringLength(20)] public string? PlacentaGranum { get; set; }
    public bool CircularCordon { get; set; }
    [StringLength(1000)] public string? Conclusiones { get; set; }
}