using Clinica.Domain.DTOs.Atenciones;

namespace Clinica.API.Services;

public interface IAtencionService
{
    Task<IEnumerable<AtencionResponseDto>> ObtenerTodasAsync();
    Task<IEnumerable<AtencionResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<AtencionResponseDto?> ObtenerPorIdAsync(Guid id);
    
    Task<Guid> RegistrarAtencionAsync(RegistrarAtencionDto dto);
    Task CerrarAtencionAsync(Guid id, CerrarAtencionDto dto);
    Task AnularAtencionAsync(Guid id, string motivo);
}