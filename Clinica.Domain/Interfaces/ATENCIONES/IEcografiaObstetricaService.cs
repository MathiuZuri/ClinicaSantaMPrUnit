using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IEcografiaObstetricaService
{
    Task<IEnumerable<EcografiaObstetricaDto>> ObtenerPorAtencionAsync(Guid atencionId);
    Task<Guid> RegistrarAsync(Guid atencionId, EcografiaObstetricaDto dto);
}