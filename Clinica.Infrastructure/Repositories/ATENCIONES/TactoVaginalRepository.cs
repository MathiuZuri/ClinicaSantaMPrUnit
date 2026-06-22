using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Interfaces.ATENCIONES;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories.ATENCIONES;

public class TactoVaginalRepository : GenericRepository<TactoVaginal>, ITactoVaginalRepository
{
    public TactoVaginalRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TactoVaginal>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        return await Context.TactosVaginales
            .Where(x => x.AtencionId == atencionId)
            .OrderByDescending(x => x.FechaHora)
            .ToListAsync();
    }
}