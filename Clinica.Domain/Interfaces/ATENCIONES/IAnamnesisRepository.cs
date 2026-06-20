using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IAnamnesisRepository : IGenericRepository<Anamnesis>
{
    // Solo hay 1 anamnesis por atención
    Task<Anamnesis?> ObtenerPorAtencionAsync(Guid atencionId);
}