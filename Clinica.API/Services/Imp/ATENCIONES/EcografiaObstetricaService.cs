using Clinica.Domain.DTOs.Atenciones.Modulos;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Interfaces.ATENCIONES;

namespace Clinica.API.Services.Imp.ATENCIONES;

public class EcografiaObstetricaService : IEcografiaObstetricaService
{
    private readonly IEcografiaObstetricaRepository _repository;

    public EcografiaObstetricaService(IEcografiaObstetricaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EcografiaObstetricaDto>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        var entidades = await _repository.ObtenerPorAtencionAsync(atencionId);
        return entidades.Select(e => new EcografiaObstetricaDto
        {
            FechaHora = e.FechaHora,
            DiametroBiparietal = e.DiametroBiparietal,
            CircunferenciaCefalica = e.CircunferenciaCefalica,
            CircunferenciaAbdominal = e.CircunferenciaAbdominal,
            LongitudFemur = e.LongitudFemur,
            PesoFetalEstimado = e.PesoFetalEstimado,
            IndiceLiquidoAmniotico = e.IndiceLiquidoAmniotico,
            PlacentaLocalizacion = e.PlacentaLocalizacion,
            PlacentaGranum = e.PlacentaGranum,
            CircularCordon = e.CircularCordon,
            Conclusiones = e.Conclusiones
        });
    }

    public async Task<Guid> RegistrarAsync(Guid atencionId, EcografiaObstetricaDto dto)
    {
        var entidad = new EcografiaObstetrica
        {
            Id = Guid.NewGuid(),
            AtencionId = atencionId,
            FechaHora = dto.FechaHora,
            DiametroBiparietal = dto.DiametroBiparietal,
            CircunferenciaCefalica = dto.CircunferenciaCefalica,
            CircunferenciaAbdominal = dto.CircunferenciaAbdominal,
            LongitudFemur = dto.LongitudFemur,
            PesoFetalEstimado = dto.PesoFetalEstimado,
            IndiceLiquidoAmniotico = dto.IndiceLiquidoAmniotico,
            PlacentaLocalizacion = dto.PlacentaLocalizacion,
            PlacentaGranum = dto.PlacentaGranum,
            CircularCordon = dto.CircularCordon,
            Conclusiones = dto.Conclusiones
        };

        await _repository.AddAsync(entidad);
        await _repository.SaveChangesAsync();

        return entidad.Id;
    }
}