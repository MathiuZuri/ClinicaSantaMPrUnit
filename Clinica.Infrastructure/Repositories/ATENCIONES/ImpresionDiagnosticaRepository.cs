using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces.ATENCIONES;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories.ATENCIONES;

public class ImpresionDiagnosticaRepository : GenericRepository<ImpresionDiagnostica>, IImpresionDiagnosticaRepository
{
    public ImpresionDiagnosticaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ImpresionDiagnostica?> ObtenerPorAtencionAsync(Guid atencionId)
    {
        return await Context.ImpresionesDiagnosticas
            .FirstOrDefaultAsync(x => x.AtencionId == atencionId);
    }
}