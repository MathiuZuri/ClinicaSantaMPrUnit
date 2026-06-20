using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IExamenFisicoService
{
    Task<IEnumerable<ExamenFisicoDto>> ObtenerPorAtencionAsync(Guid atencionId);
    Task<Guid> RegistrarAsync(Guid atencionId, ExamenFisicoDto dto);
}