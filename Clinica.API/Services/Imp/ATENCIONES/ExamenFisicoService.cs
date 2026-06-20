using Clinica.Domain.DTOs.Atenciones.Modulos;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces.ATENCIONES;

namespace Clinica.API.Services.Imp.ATENCIONES;

public class ExamenFisicoService : IExamenFisicoService
{
    private readonly IExamenFisicoRepository _repository;

    public ExamenFisicoService(IExamenFisicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ExamenFisicoDto>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        var entidades = await _repository.ObtenerPorAtencionAsync(atencionId);
        return entidades.Select(e => new ExamenFisicoDto
        {
            FechaHoraExamen = e.FechaHoraExamen,
            Lotep = e.Lotep,
            EstadoGeneral = e.EstadoGeneral,
            EstadoHidratacion = e.EstadoHidratacion,
            EstadoNutricion = e.EstadoNutricion,
            EscalaGlasgow = e.EscalaGlasgow,
            UteroGravido = e.UteroGravido,
            AlturaUterina = e.AlturaUterina,
            SituacionPosicionPresentacion = e.SituacionPosicionPresentacion,
            LatidosCardiacosFetales = e.LatidosCardiacosFetales,
            MovimientosFetales = e.MovimientosFetales,
            TonoUterino = e.TonoUterino,
            DinamicaUterina = e.DinamicaUterina,
            SangradoTv = e.SangradoTv,
            PerdidaLiquidoAmniotico = e.PerdidaLiquidoAmniotico,
            ColorLiquidoAmniotico = e.ColorLiquidoAmniotico,
            TaponMucoso = e.TaponMucoso,
            FlujoVaginal = e.FlujoVaginal,
            PunoPercusionLumbar = e.PunoPercusionLumbar,
            Edemas = e.Edemas,
            ReflejosOsteotendinosos = e.ReflejosOsteotendinosos
        });
    }

    public async Task<Guid> RegistrarAsync(Guid atencionId, ExamenFisicoDto dto)
    {
        // En este caso NO verificamos si existe, porque pueden haber múltiples exámenes físicos en una misma atención
        var entidad = new ExamenFisico
        {
            Id = Guid.NewGuid(),
            AtencionId = atencionId,
            FechaHoraExamen = dto.FechaHoraExamen,
            Lotep = dto.Lotep,
            EstadoGeneral = dto.EstadoGeneral,
            EstadoHidratacion = dto.EstadoHidratacion,
            EstadoNutricion = dto.EstadoNutricion,
            EscalaGlasgow = dto.EscalaGlasgow,
            UteroGravido = dto.UteroGravido,
            AlturaUterina = dto.AlturaUterina,
            SituacionPosicionPresentacion = dto.SituacionPosicionPresentacion,
            LatidosCardiacosFetales = dto.LatidosCardiacosFetales,
            MovimientosFetales = dto.MovimientosFetales,
            TonoUterino = dto.TonoUterino,
            DinamicaUterina = dto.DinamicaUterina,
            SangradoTv = dto.SangradoTv,
            PerdidaLiquidoAmniotico = dto.PerdidaLiquidoAmniotico,
            ColorLiquidoAmniotico = dto.ColorLiquidoAmniotico,
            TaponMucoso = dto.TaponMucoso,
            FlujoVaginal = dto.FlujoVaginal,
            PunoPercusionLumbar = dto.PunoPercusionLumbar,
            Edemas = dto.Edemas,
            ReflejosOsteotendinosos = dto.ReflejosOsteotendinosos
        };

        await _repository.AddAsync(entidad);
        await _repository.SaveChangesAsync();

        return entidad.Id;
    }
}