using Clinica.Domain.DTOs.Atenciones.Modulos;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Interfaces.ATENCIONES;

namespace Clinica.API.Services.Imp.ATENCIONES;

public class ImpresionDiagnosticaService : IImpresionDiagnosticaService
{
    private readonly IImpresionDiagnosticaRepository _repository;

    public ImpresionDiagnosticaService(IImpresionDiagnosticaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ImpresionDiagnosticaDto?> ObtenerPorAtencionAsync(Guid atencionId)
    {
        var entidad = await _repository.ObtenerPorAtencionAsync(atencionId);
        if (entidad == null) return null;

        return new ImpresionDiagnosticaDto
        {
            DiagnosticoPrincipal = entidad.DiagnosticoPrincipal,
            DiagnosticosSecundarios = entidad.DiagnosticosSecundarios,
            IndicacionesReceta = entidad.IndicacionesReceta,
            FechaProximaCita = entidad.FechaProximaCita,
            MotivoProximaCita = entidad.MotivoProximaCita
        };
    }

    public async Task<Guid> RegistrarAsync(Guid atencionId, ImpresionDiagnosticaDto dto)
    {
        var existente = await _repository.ObtenerPorAtencionAsync(atencionId);
        if (existente != null) throw new InvalidOperationException("Esta atención ya tiene un diagnóstico final.");

        var entidad = new ImpresionDiagnostica
        {
            Id = Guid.NewGuid(),
            AtencionId = atencionId,
            DiagnosticoPrincipal = dto.DiagnosticoPrincipal,
            DiagnosticosSecundarios = dto.DiagnosticosSecundarios,
            IndicacionesReceta = dto.IndicacionesReceta,
            FechaProximaCita = dto.FechaProximaCita,
            MotivoProximaCita = dto.MotivoProximaCita
        };

        await _repository.AddAsync(entidad);
        await _repository.SaveChangesAsync();

        return entidad.Id;
    }
}