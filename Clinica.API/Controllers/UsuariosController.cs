using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Usuarios;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [Authorize(Policy = PermisosPolicies.UsuarioVer)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _usuarioService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(usuarios, "Usuarios obtenidos correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.UsuarioVer)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(id);

        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        return Ok(ApiResponse<object>.Ok(usuario, "Usuario obtenido correctamente."));
    }

    [Auditoria("Usuarios", "Usuario", TipoAccionAuditoria.Creacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioCrear)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearUsuarioDto dto)
    {
        var id = await _usuarioService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Usuario creado correctamente.", 201)
        );
    }

    [Auditoria("Usuarios", "Usuario", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioEditar)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarUsuarioDto dto)
    {
        await _usuarioService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Usuario actualizado correctamente."));
    }

    [Auditoria("Usuarios", "UsuarioRol", TipoAccionAuditoria.Asignacion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioAsignarRol)]
    [HttpPost("asignar-rol")]
    public async Task<IActionResult> AssignRole([FromBody] AsignarRolUsuarioDto dto)
    {
        await _usuarioService.AsignarRolAsync(dto);
        return Ok(ApiResponse<object>.Ok(dto, "Rol asignado correctamente."));
    }
    
    [Auditoria("Usuarios", "Usuario", TipoAccionAuditoria.Edicion, NivelAuditoria.Critico)]
    [Authorize(Policy = PermisosPolicies.UsuarioEditar)]
    [HttpPut("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoUsuarioDto dto)
    {
        await _usuarioService.CambiarEstadoAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Estado del usuario actualizado correctamente."));
    }
}