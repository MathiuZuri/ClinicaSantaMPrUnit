using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface ITactoVaginalService
{
    Task<IEnumerable<TactoVaginalDto>> ObtenerPorAtencionAsync(Guid atencionId);
    Task<Guid> RegistrarAsync(Guid atencionId, TactoVaginalDto dto);
}