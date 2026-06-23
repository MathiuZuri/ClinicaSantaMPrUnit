using Clinica.Domain.DTOs.Auditoria;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.Enums;

namespace Clinica.API.Services;

public interface IAuditoriaService
{
    Task<PaginacionResponseDto<AuditoriaResponseDto>> ObtenerTodosPaginadosAsync(
        PaginacionRequestDto request,
        TipoAccionAuditoria? tipoAccion = null,
        bool? soloConsultas = null
    );

    Task<PaginacionResponseDto<AuditoriaResponseDto>> ObtenerPorUsuarioPaginadosAsync(
        Guid usuarioId,
        PaginacionRequestDto request,
        TipoAccionAuditoria? tipoAccion = null,
        bool? soloConsultas = null
    );
}