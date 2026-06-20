using Clinica.API.Authorization;
using Clinica.API.Models;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.Domain.PDFsDto;
using Clinica.Domain.PDFsDto.Interfacespdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace Clinica.API.Controllers.pdfControladores;

[ApiController]
[Route("api/[controller]")]
[Tags("Documentos PDF - Certificados")]
public class CertificadosController : ControllerBase
{
    private readonly ICertificadoTrabajoPdfService _pdfService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUsuarioActualService _usuarioActualService;

    public CertificadosController(
        ICertificadoTrabajoPdfService pdfService,
        IUsuarioRepository usuarioRepository,
        IDoctorRepository doctorRepository,
        IUsuarioActualService usuarioActualService)
    {
        _pdfService = pdfService;
        _usuarioRepository = usuarioRepository;
        _doctorRepository = doctorRepository;
        _usuarioActualService = usuarioActualService;
    }

    /// <summary>
    /// Descarga el certificado de trabajo del usuario autenticado.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite a un usuario obtener su propio certificado laboral en PDF.
    /// **Permiso requerido:** <see cref="PermisosPolicies.CertificadoGenerar"/>.
    /// </remarks>
    [HttpGet("mi-certificado")]
    [Authorize(Policy = PermisosPolicies.CertificadoGenerar)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarMiCertificado()
    {
        var usuarioId = _usuarioActualService.ObtenerUsuarioId();
        if (usuarioId == Guid.Empty)
            return Unauthorized(ApiResponse<object>.Error("Usuario no autenticado", 401));

        var dto = await ConstruirDtoDesdeUsuario(usuarioId);
        if (dto == null)
            return NotFound(ApiResponse<object>.Error("Usuario no encontrado", 404));

        var pdfBytes = _pdfService.GeneratePdf(dto);
        var fileName = $"CertificadoTrabajo_{dto.CodigoUsuario}_{DateTime.Now:yyyyMMddHHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Genera certificados de trabajo en bloque para múltiples usuarios y los devuelve en un ZIP.
    /// </summary>
    /// <remarks>
    /// **Uso:** Permite a administradores o directores generar certificados para un grupo de usuarios
    /// (por lista de IDs o por nombre de rol). Útil para procesos de entrega masiva.
    /// **Permiso requerido:** <see cref="PermisosPolicies.CertificadoBlock"/>.
    /// </remarks>
    [HttpPost("block")]
    [Authorize(Policy = PermisosPolicies.CertificadoBlock)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarCertificadosEnBloque([FromBody] CertificadoBlockRequest request)
    {
        // Validaciones
        if (request == null || (request.UsuarioIds == null && string.IsNullOrWhiteSpace(request.Rol)))
            return BadRequest(ApiResponse<object>.Error("Debe especificar al menos un usuario o un rol", 400));

        List<Usuario> usuarios;

        if (request.UsuarioIds != null && request.UsuarioIds.Any())
        {
            var usuariosTemp = new List<Usuario>();
            foreach (var id in request.UsuarioIds)
            {
                var u = await _usuarioRepository.GetByIdAsync(id);
                if (u != null) usuariosTemp.Add(u);
            }
            usuarios = usuariosTemp;
        }
        else
        {
            var todos = await _usuarioRepository.GetAllAsync();
            usuarios = todos
                .Where(u => u.UsuarioRoles.Any(ur => ur.Activo && ur.Rol.Nombre == request.Rol))
                .ToList();
        }

        if (!usuarios.Any())
            return BadRequest(ApiResponse<object>.Error("No se encontraron usuarios para los criterios especificados", 400));

        var pdfs = new List<byte[]>();
        var codigos = new List<string>();
        foreach (var usuario in usuarios)
        {
            var dto = await ConstruirDtoDesdeUsuario(usuario.Id);
            if (dto != null)
            {
                pdfs.Add(_pdfService.GeneratePdf(dto));
                codigos.Add(dto.CodigoUsuario);
            }
        }

        if (!pdfs.Any())
            return BadRequest(ApiResponse<object>.Error("No se pudo generar ningún certificado", 400));

        var zipBytes = CrearZipConCertificados(pdfs, codigos);
        return File(zipBytes, "application/zip", $"Certificados_{DateTime.Now:yyyyMMddHHmm}.zip");
    }

    // =====================================================================
    // Helpers privados
    // =====================================================================
    private async Task<CertificadoTrabajoDto?> ConstruirDtoDesdeUsuario(Guid usuarioId)
    {
        var usuario = await _usuarioRepository.ObtenerConRolesAsync(usuarioId);
        if (usuario == null) return null;

        var roles = usuario.UsuarioRoles
            .Where(ur => ur.Activo)
            .Select(ur => ur.Rol.Nombre)
            .ToList();

        var doctor = await ObtenerDoctorPorUsuarioId(usuarioId);

        string area = roles.FirstOrDefault() ?? "General";
        string cargo = "";
        if (doctor != null)
        {
            area = doctor.Especialidad ?? area;
            cargo = $"Médico {doctor.Especialidad}";
        }
        else
        {
            cargo = roles.FirstOrDefault() ?? "Personal de Clínica";
        }

        var fechaInicio = usuario.FechaRegistro;
        var fechaFin = DateTime.UtcNow;
        string dni = "Pendiente"; // Pendiente de agregar DNI a Usuario
        string codigoCertificado = $"CERT-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString("N").Substring(0,8).ToUpper()}";
        string observaciones = "Certificado emitido por el sistema de gestión clínica.";

        return new CertificadoTrabajoDto
        {
            NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
            Dni = dni,
            CodigoUsuario = usuario.CodigoUsuario,
            Correo = usuario.Correo,
            Roles = roles,
            Area = area,
            Cargo = cargo,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            NombreDirector = "Dr. Juan Pérez",   // Puede venir de configuración
            CargoDirector = "Director Médico",
            CodigoCertificado = codigoCertificado,
            Observaciones = observaciones
        };
    }

    private async Task<Doctor?> ObtenerDoctorPorUsuarioId(Guid usuarioId)
    {
        var doctores = await _doctorRepository.GetAllAsync();
        return doctores.FirstOrDefault(d => d.UsuarioId == usuarioId);
    }

    private byte[] CrearZipConCertificados(List<byte[]> pdfs, List<string> codigos)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            for (int i = 0; i < pdfs.Count; i++)
            {
                var entry = zip.CreateEntry($"Certificado_{codigos[i]}_{DateTime.Now:yyyyMMddHHmm}.pdf");
                using var entryStream = entry.Open();
                entryStream.Write(pdfs[i], 0, pdfs[i].Length);
            }
        }
        return ms.ToArray();
    }
}

// DTO para la solicitud en bloque
public class CertificadoBlockRequest
{
    public List<Guid>? UsuarioIds { get; set; }
    public string? Rol { get; set; } // Ej: "Doctor", "Recepcionista"
}