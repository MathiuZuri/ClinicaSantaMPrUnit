using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Roles;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [Authorize(Policy = PermisosPolicies.RolVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _rolService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(roles, "Roles obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.RolVer)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var rol = await _rolService.ObtenerPorIdAsync(id);

        if (rol == null)
            throw new KeyNotFoundException("Rol no encontrado.");

        return Ok(ApiResponse<object>.Ok(rol, "Rol obtenido correctamente."));
    }

    [Auditoria("Roles", "Rol", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.RolCrear)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearRolDto dto)
    {
        var id = await _rolService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Rol creado correctamente.", 201)
        );
    }

    [Auditoria("Roles", "Rol", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.RolEditar)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarRolDto dto)
    {
        await _rolService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Rol actualizado correctamente."));
    }

    [Auditoria("Roles", "RolPermiso", TipoAccionAuditoria.Asignacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.RolAsignarPermisos)]
    [HttpPost("asignar-permisos")]
    public async Task<IActionResult> AssignPermissions([FromBody] AsignarPermisosRolDto dto)
    {
        await _rolService.AsignarPermisosAsync(dto);
        return Ok(ApiResponse<object>.Ok(dto, "Permisos asignados correctamente."));
    }
}