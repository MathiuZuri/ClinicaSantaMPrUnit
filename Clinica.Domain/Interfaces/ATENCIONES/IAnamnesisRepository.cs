using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IAnamnesisRepository : IGenericRepository<Anamnesis>
{
    // Solo hay 1 anamnesis por atención
    Task<Anamnesis?> ObtenerPorAtencionAsync(Guid atencionId);
}