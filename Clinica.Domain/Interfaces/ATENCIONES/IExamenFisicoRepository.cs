using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IExamenFisicoRepository : IGenericRepository<ExamenFisico>
{
    // Puede haber múltiples exámenes físicos en una misma atención (monitoreo)
    Task<IEnumerable<ExamenFisico>> ObtenerPorAtencionAsync(Guid atencionId);
}