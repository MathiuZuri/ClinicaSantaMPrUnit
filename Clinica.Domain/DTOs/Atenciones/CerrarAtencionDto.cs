using System.ComponentModel.DataAnnotations;
using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.Domain.DTOs.Atenciones;

public class CerrarAtencionDto
{
    [Required(ErrorMessage = "La impresión diagnóstica es obligatoria para cerrar la atención.")]
    public ImpresionDiagnosticaDto ImpresionDiagnostica { get; set; } = new();

    [StringLength(1000, ErrorMessage = "Las observaciones finales no deben superar los 1000 caracteres.")]
    public string? ObservacionesFinales { get; set; }
}