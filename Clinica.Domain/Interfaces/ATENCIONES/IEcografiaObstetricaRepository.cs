using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IEcografiaObstetricaRepository : IGenericRepository<EcografiaObstetrica>
{
    Task<IEnumerable<EcografiaObstetrica>> ObtenerPorAtencionAsync(Guid atencionId);
}