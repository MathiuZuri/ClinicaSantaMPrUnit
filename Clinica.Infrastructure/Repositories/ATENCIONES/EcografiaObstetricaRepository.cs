using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Interfaces.ATENCIONES;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories.ATENCIONES;

public class EcografiaObstetricaRepository : GenericRepository<EcografiaObstetrica>, IEcografiaObstetricaRepository
{
    public EcografiaObstetricaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EcografiaObstetrica>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        return await Context.EcografiasObstetricas
            .Where(x => x.AtencionId == atencionId)
            .OrderByDescending(x => x.FechaHora)
            .ToListAsync();
    }
}