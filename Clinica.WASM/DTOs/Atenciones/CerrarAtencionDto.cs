using System.ComponentModel.DataAnnotations;
using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.WASM.DTOs.Atenciones;

public class CerrarAtencionDto
{
    [Required] public ImpresionDiagnosticaDto ImpresionDiagnostica { get; set; } = new();
    [StringLength(1000)] public string? ObservacionesFinales { get; set; }
}