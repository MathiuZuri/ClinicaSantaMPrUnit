using System.ComponentModel.DataAnnotations;

namespace Clinica.WASM.DTOs.Atenciones;

public class ExamenFisicoDto
{
    public DateTime? FechaHoraExamen { get; set; } = DateTime.UtcNow;
    public bool Lotep { get; set; }
    [StringLength(50)] public string? EstadoGeneral { get; set; }
    [StringLength(50)] public string? EstadoHidratacion { get; set; }
    [StringLength(50)] public string? EstadoNutricion { get; set; }
    public int? EscalaGlasgow { get; set; }
    public bool UteroGravido { get; set; }
    public int? AlturaUterina { get; set; }
    [StringLength(100)] public string? SituacionPosicionPresentacion { get; set; }
    public int? LatidosCardiacosFetales { get; set; }
    [StringLength(50)] public string? MovimientosFetales { get; set; }
    [StringLength(50)] public string? TonoUterino { get; set; }
    [StringLength(100)] public string? DinamicaUterina { get; set; }
    public bool SangradoTv { get; set; }
    public bool PerdidaLiquidoAmniotico { get; set; }
    [StringLength(50)] public string? ColorLiquidoAmniotico { get; set; }
    public bool TaponMucoso { get; set; }
    public bool FlujoVaginal { get; set; }
    [StringLength(50)] public string? PunoPercusionLumbar { get; set; }
    [StringLength(50)] public string? Edemas { get; set; }
    [StringLength(50)] public string? ReflejosOsteotendinosos { get; set; }
}