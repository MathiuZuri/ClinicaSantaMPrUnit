using System.ComponentModel.DataAnnotations;
using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.WASM.DTOs.Atenciones;

public class RegistrarAtencionDto
{
    [Required] public Guid PacienteId { get; set; }
    [Required] public Guid DoctorId { get; set; }
    [Required] public Guid ServicioClinicoId { get; set; }
    public Guid? CitaId { get; set; }
    public Guid? HistorialClinicoId { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal CostoFinal { get; set; }

    public AnamnesisDto? Anamnesis { get; set; }
    public List<ExamenFisicoDto>? ExamenesFisicos { get; set; }
    public List<TactoVaginalDto>? TactosVaginales { get; set; }
    public List<EcografiaObstetricaDto>? Ecografias { get; set; }
    public ImpresionDiagnosticaDto? ImpresionDiagnostica { get; set; }
}