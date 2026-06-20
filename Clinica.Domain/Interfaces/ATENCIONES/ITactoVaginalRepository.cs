using Clinica.Domain.Entities;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface ITactoVaginalRepository : IGenericRepository<TactoVaginal>
{
    Task<IEnumerable<TactoVaginal>> ObtenerPorAtencionAsync(Guid atencionId);
}