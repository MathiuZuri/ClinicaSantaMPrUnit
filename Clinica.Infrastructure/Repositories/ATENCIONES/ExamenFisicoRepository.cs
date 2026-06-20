using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces.ATENCIONES;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories.ATENCIONES;

public class ExamenFisicoRepository : GenericRepository<ExamenFisico>, IExamenFisicoRepository
{
    public ExamenFisicoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExamenFisico>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        // Traemos todos los exámenes físicos de esta atención, ordenados por fecha de más reciente a más antiguo
        return await Context.ExamenesFisicos
            .Where(x => x.AtencionId == atencionId)
            .OrderByDescending(x => x.FechaHoraExamen)
            .ToListAsync();
    }
}