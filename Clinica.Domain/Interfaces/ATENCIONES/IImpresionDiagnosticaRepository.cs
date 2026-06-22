using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IImpresionDiagnosticaRepository : IGenericRepository<ImpresionDiagnostica>
{
    // Solo hay 1 diagnóstico final por atención
    Task<ImpresionDiagnostica?> ObtenerPorAtencionAsync(Guid atencionId);
}