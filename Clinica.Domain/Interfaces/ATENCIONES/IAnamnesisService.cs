using Clinica.Domain.DTOs.Atenciones.Modulos;

namespace Clinica.Domain.Interfaces.ATENCIONES;

public interface IAnamnesisService
{
    Task<AnamnesisDto?> ObtenerPorAtencionAsync(Guid atencionId);
    Task<Guid> RegistrarAsync(Guid atencionId, AnamnesisDto dto);
}