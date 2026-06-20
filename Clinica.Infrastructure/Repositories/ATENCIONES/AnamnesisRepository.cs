using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces.ATENCIONES;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories.ATENCIONES;

public class AnamnesisRepository : GenericRepository<Anamnesis>, IAnamnesisRepository
{
    public AnamnesisRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Anamnesis?> ObtenerPorAtencionAsync(Guid atencionId)
    {
        return await Context.Anamnesis
            .FirstOrDefaultAsync(x => x.AtencionId == atencionId);
    }
}