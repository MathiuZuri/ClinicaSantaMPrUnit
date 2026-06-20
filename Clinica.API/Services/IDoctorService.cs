using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.Enums;

namespace Clinica.API.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorResponseDto>> ObtenerTodosAsync();
    Task<IEnumerable<DoctorResponseDto>> ObtenerActivosAsync();
    Task<DoctorResponseDto?> ObtenerPorIdAsync(Guid id);
    Task<Guid> CrearAsync(CrearDoctorDto dto);
    Task ActualizarAsync(Guid id, EditarDoctorDto dto);

    // NUEVOS
    Task<Guid> ContratarAsync(ContratarDoctorDto dto);
    Task<PaginacionResponseDto<DoctorResponseDto>> BuscarAsync(
        string? nombre,
        string? especialidad,
        EstadoDoctor? estado,
        PaginacionRequestDto request);
}