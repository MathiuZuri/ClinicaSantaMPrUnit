using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class AtencionRepository : GenericRepository<Atencion>, IAtencionRepository
{
    public AtencionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Atencion>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        return await Context.Atenciones
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
                .ThenInclude(d => d.Usuario) // Importante para obtener el nombre del doctor
            .Include(x => x.ServicioClinico)
            .Include(x => x.Cita)
            // Para el listado general del paciente, solo traemos módulos rápidos (sin traer las colecciones pesadas)
            .Include(x => x.Anamnesis)
            .Include(x => x.ImpresionDiagnostica)
            .Where(x => x.PacienteId == pacienteId)
            .OrderByDescending(x => x.FechaInicio)
            .ToListAsync();
    }

    public async Task<Atencion?> ObtenerPorCitaAsync(Guid citaId)
    {
        return await Context.Atenciones
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
            .Include(x => x.ServicioClinico)
            .Include(x => x.Cita)
            .FirstOrDefaultAsync(x => x.CitaId == citaId);
    }

    // ==========================================================
    // MÉTODO PARA TRAER TODO EL EXPEDIENTE (CORE + MÓDULOS)
    // ==========================================================
    public async Task<Atencion?> ObtenerDetalleCompletoAsync(Guid id)
    {
        return await Context.Atenciones
            .Include(x => x.Paciente)
            .Include(x => x.Doctor)
                .ThenInclude(d => d.Usuario)
            .Include(x => x.ServicioClinico)
            .Include(x => x.Cita)
            .Include(x => x.Pagos)
            // --- INCLUDES MODULARES INDEPENDIENTES ---
            .Include(x => x.Anamnesis)
            .Include(x => x.ExamenesFisicos)
            .Include(x => x.TactosVaginales)
            .Include(x => x.Ecografias)
            .Include(x => x.ImpresionDiagnostica)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<IEnumerable<Atencion>> ObtenerTodasConRelacionesAsync()
    {
        return await Context.Atenciones
            .Include(a => a.Paciente)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.Usuario)
            .Include(a => a.ServicioClinico)
            .Include(a => a.Pagos)
            .AsNoTracking()
            .ToListAsync();
    }
}