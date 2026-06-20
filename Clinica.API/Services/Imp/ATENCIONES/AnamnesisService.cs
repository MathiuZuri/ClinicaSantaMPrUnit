using Clinica.Domain.DTOs.Atenciones.Modulos;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces.ATENCIONES;

namespace Clinica.API.Services.Imp.ATENCIONES;

public class AnamnesisService : IAnamnesisService
{
    private readonly IAnamnesisRepository _repository;

    public AnamnesisService(IAnamnesisRepository repository)
    {
        _repository = repository;
    }

    public async Task<AnamnesisDto?> ObtenerPorAtencionAsync(Guid atencionId)
    {
        var entidad = await _repository.ObtenerPorAtencionAsync(atencionId);
        if (entidad == null) return null;

        return new AnamnesisDto
        {
            MotivoConsulta = entidad.MotivoConsulta,
            Gestaciones = entidad.Gestaciones,
            HijosVivos = entidad.HijosVivos,
            Abortos = entidad.Abortos,
            PartosPretermino = entidad.PartosPretermino,
            PartosATermino = entidad.PartosATermino,
            FechaUltimaRegla = entidad.FechaUltimaRegla,
            FechaProbableParto = entidad.FechaProbableParto,
            EdadGestacional = entidad.EdadGestacional,
            Alergias = entidad.Alergias,
            EnfermedadesCronicas = entidad.EnfermedadesCronicas,
            CirugiasPrevias = entidad.CirugiasPrevias,
            AntecedentesAdicionales = entidad.AntecedentesAdicionales
        };
    }

    public async Task<Guid> RegistrarAsync(Guid atencionId, AnamnesisDto dto)
    {
        // Verificamos si ya existe (es 1 a 1)
        var existente = await _repository.ObtenerPorAtencionAsync(atencionId);
        if (existente != null) throw new InvalidOperationException("Esta atención ya tiene una anamnesis registrada.");

        var anamnesis = new Anamnesis
        {
            Id = Guid.NewGuid(),
            AtencionId = atencionId,
            MotivoConsulta = dto.MotivoConsulta,
            Gestaciones = dto.Gestaciones,
            HijosVivos = dto.HijosVivos,
            Abortos = dto.Abortos,
            PartosPretermino = dto.PartosPretermino,
            PartosATermino = dto.PartosATermino,
            FechaUltimaRegla = dto.FechaUltimaRegla,
            FechaProbableParto = dto.FechaProbableParto,
            EdadGestacional = dto.EdadGestacional,
            Alergias = dto.Alergias,
            EnfermedadesCronicas = dto.EnfermedadesCronicas,
            CirugiasPrevias = dto.CirugiasPrevias,
            AntecedentesAdicionales = dto.AntecedentesAdicionales
        };

        await _repository.AddAsync(anamnesis);
        await _repository.SaveChangesAsync();

        return anamnesis.Id;
    }
}