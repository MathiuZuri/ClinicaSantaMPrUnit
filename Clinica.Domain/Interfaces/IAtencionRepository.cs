using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;

namespace Clinica.Domain.Interfaces;

public interface IAtencionRepository : IGenericRepository<Atencion>
{
    Task<IEnumerable<Atencion>> ObtenerPorPacienteAsync(Guid pacienteId);
    Task<Atencion?> ObtenerPorCitaAsync(Guid citaId);

    Task<Atencion?> ObtenerDetalleCompletoAsync(Guid id);
    
    Task<IEnumerable<Atencion>> ObtenerTodasConRelacionesAsync();
}