using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IEcografiaObstetricaRepository : IGenericRepository<EcografiaObstetrica>
{
    Task<IEnumerable<EcografiaObstetrica>> ObtenerPorAtencionAsync(Guid atencionId);
}