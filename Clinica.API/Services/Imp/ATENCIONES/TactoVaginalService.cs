using Clinica.Domain.DTOs.Atenciones.Modulos;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Interfaces.ATENCIONES;

namespace Clinica.API.Services.Imp.ATENCIONES;

public class TactoVaginalService : ITactoVaginalService
{
    private readonly ITactoVaginalRepository _repository;

    public TactoVaginalService(ITactoVaginalRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TactoVaginalDto>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        var entidades = await _repository.ObtenerPorAtencionAsync(atencionId);
        return entidades.Select(t => new TactoVaginalDto
        {
            FechaHora = t.FechaHora,
            Dilatacion = t.Dilatacion,
            Borramiento = t.Borramiento,
            AlturaPresentacion = t.AlturaPresentacion,
            MembranasOvulares = t.MembranasOvulares,
            ColorLiquido = t.ColorLiquido,
            Pelvis = t.Pelvis,
            VariedadPresentacion = t.VariedadPresentacion
        });
    }

    public async Task<Guid> RegistrarAsync(Guid atencionId, TactoVaginalDto dto)
    {
        var entidad = new TactoVaginal
        {
            Id = Guid.NewGuid(),
            AtencionId = atencionId,
            FechaHora = dto.FechaHora,
            Dilatacion = dto.Dilatacion,
            Borramiento = dto.Borramiento,
            AlturaPresentacion = dto.AlturaPresentacion,
            MembranasOvulares = dto.MembranasOvulares,
            ColorLiquido = dto.ColorLiquido,
            Pelvis = dto.Pelvis,
            VariedadPresentacion = dto.VariedadPresentacion
        };

        await _repository.AddAsync(entidad);
        await _repository.SaveChangesAsync();

        return entidad.Id;
    }
}