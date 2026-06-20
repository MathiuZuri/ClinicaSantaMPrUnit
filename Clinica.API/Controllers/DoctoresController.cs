using Clinica.API.Authorization;
using Clinica.API.Filters;
using Clinica.API.Models;
using Clinica.API.Services;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

/// <summary>
/// Controlador para la gestión del cuerpo médico (doctores).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Gestión de Doctores")]
public class DoctoresController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctoresController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Obtiene la lista de todos los doctores registrados.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite consultar el catálogo completo de doctores.
    /// **Permiso requerido:** <see cref="PermisosPolicies.DoctorVer"/>.
    /// </remarks>
    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var doctores = await _doctorService.ObtenerTodosAsync();
        return Ok(ApiResponse<object>.Ok(doctores, "Doctores obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene la lista de doctores activos.
    /// </summary>
    /// <remarks>
    /// **Uso:** Filtra solo doctores con estado "Activo", útiles para programar citas.
    /// **Permiso requerido:** <see cref="PermisosPolicies.DoctorVer"/>.
    /// </remarks>
    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet("activos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivos()
    {
        var doctores = await _doctorService.ObtenerActivosAsync();
        return Ok(ApiResponse<object>.Ok(doctores, "Doctores activos obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene los datos de un doctor por su ID.
    /// </summary>
    /// <remarks>
    /// **Uso:** Recupera la información completa de un doctor específico.
    /// **Permiso requerido:** <see cref="PermisosPolicies.DoctorVer"/>.
    /// </remarks>
    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var doctor = await _doctorService.ObtenerPorIdAsync(id);
        if (doctor == null)
            throw new KeyNotFoundException("Doctor no encontrado.");
        return Ok(ApiResponse<object>.Ok(doctor, "Doctor obtenido correctamente."));
    }

    /// <summary>
    /// Registra un nuevo doctor en el sistema.
    /// </summary>
    /// <remarks>
    /// **Uso:** Crea un nuevo doctor con sus datos personales y CMP.
    /// **Permiso requerido:** <see cref="PermisosPolicies.DoctorCrear"/>.
    /// </remarks>
    [Authorize(Policy = PermisosPolicies.DoctorCrear)]
    [Auditoria("Doctores", "Doctor", TipoAccionAuditoria.Creacion, NivelAuditoria.Importante)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CrearDoctorDto dto)
    {
        var id = await _doctorService.CrearAsync(dto);
        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<object>.Ok(new { id }, "Doctor registrado correctamente.", 201)
        );
    }

    /// <summary>
    /// Actualiza los datos de un doctor existente.
    /// </summary>
    /// <remarks>
    /// **Uso:** Modifica los datos personales, especialidad, estado, etc. de un doctor.
    /// **Permiso requerido:** <see cref="PermisosPolicies.DoctorEditar"/>.
    /// </remarks>
    [Authorize(Policy = PermisosPolicies.DoctorEditar)]
    [Auditoria("Doctores", "Doctor", TipoAccionAuditoria.Edicion, NivelAuditoria.Importante)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarDoctorDto dto)
    {
        await _doctorService.ActualizarAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { id }, "Doctor actualizado correctamente."));
    }

    /// <summary>
    /// Contrata un nuevo médico creando también su usuario y asignándole rol Doctor.
    /// </summary>
    /// <remarks>
    /// **Uso:** Registra un médico y, a la vez, crea su usuario en el sistema.
    /// **Permiso requerido:** <see cref="PermisosPolicies.DoctorCrear"/>.
    /// </remarks>
    [Authorize(Policy = PermisosPolicies.DoctorCrear)]
    [Auditoria("Doctores", "Doctor", TipoAccionAuditoria.Creacion, NivelAuditoria.Importante)]
    [HttpPost("contratar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Contratar([FromBody] ContratarDoctorDto dto)
    {
        var id = await _doctorService.ContratarAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<object>.Ok(new { id }, "Médico contratado y usuario creado exitosamente.", 201));
    }

    /// <summary>
    /// Busca doctores por nombre, especialidad o estado, con paginación.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite búsquedas avanzadas con filtros opcionales.
    /// **Permiso requerido:** <see cref="PermisosPolicies.DoctorVer"/>.
    /// </remarks>
    [Authorize(Policy = PermisosPolicies.DoctorVer)]
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ApiResponse<PaginacionResponseDto<DoctorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? nombre,
        [FromQuery] string? especialidad,
        [FromQuery] EstadoDoctor? estado,
        [FromQuery] PaginacionRequestDto request)
    {
        var resultado = await _doctorService.BuscarAsync(nombre, especialidad, estado, request);
        return Ok(ApiResponse<object>.Ok(resultado, "Doctores encontrados."));
    }
}