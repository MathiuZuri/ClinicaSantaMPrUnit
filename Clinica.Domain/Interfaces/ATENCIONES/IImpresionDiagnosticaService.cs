using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IImpresionDiagnosticaService
{
    Task<ImpresionDiagnosticaDto?> ObtenerPorAtencionAsync(Guid atencionId);
    Task<Guid> RegistrarAsync(Guid atencionId, ImpresionDiagnosticaDto dto);
}