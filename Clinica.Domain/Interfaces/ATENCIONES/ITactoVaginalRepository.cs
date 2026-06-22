using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface ITactoVaginalRepository : IGenericRepository<TactoVaginal>
{
    Task<IEnumerable<TactoVaginal>> ObtenerPorAtencionAsync(Guid atencionId);
}