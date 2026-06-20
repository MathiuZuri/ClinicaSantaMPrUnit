using Clinica.API.Helpers;
using Clinica.Domain.DTOs.Auth;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    private readonly JwtHelper _jwtHelper;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IUsuarioActualService usuarioActualService,
        JwtHelper jwtHelper)
    {
        _usuarioRepository = usuarioRepository;
        _usuarioActualService = usuarioActualService;
        _jwtHelper = jwtHelper;
    }

    public async Task<RespuestaInicioSesionDto> IniciarSesionAsync(IniciarSesionDto dto)
    {
        var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(dto.UsuarioOCorreo)
                      ?? await _usuarioRepository.ObtenerPorUserNameAsync(dto.UsuarioOCorreo);

        if (usuario == null)
            throw new InvalidOperationException("Usuario o contraseña incorrectos.");
        
        if (usuario.Estado != EstadoUsuario.Activo)
            throw new InvalidOperationException("Tu cuenta no está activa. Contacta al administrador.");

        var passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

        if (!passwordValido)
            throw new InvalidOperationException("Usuario o contraseña incorrectos.");

        var roles = usuario.UsuarioRoles
            .Where(x => x.Activo)
            .Select(x => x.Rol.Nombre)
            .Distinct()
            .ToList();

        var permisos = usuario.UsuarioRoles
            .Where(x => x.Activo)
            .SelectMany(x => x.Rol.RolPermisos)
            .Select(x => x.Permiso.Codigo)
            .Distinct()
            .ToList();

        var token = _jwtHelper.GenerarToken(usuario, roles, permisos);

        return new RespuestaInicioSesionDto
        {
            UsuarioId = usuario.Id,
            CodigoUsuario = usuario.CodigoUsuario,
            NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
            Correo = usuario.Correo,
            Token = token,
            Roles = roles,
            Permisos = permisos,
            DebeCambiarContrasena = usuario.DebeCambiarContrasena || usuario.UltimoAcceso == null
        };
    }
    
    public async Task CambiarContrasenaAsync(CambiarContrasenaDto dto)
    {
        var usuarioId = _usuarioActualService.ObtenerUsuarioId();
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        // Verificar contraseña actual
        if (!BCrypt.Net.BCrypt.Verify(dto.ContrasenaActual, usuario.PasswordHash))
            throw new InvalidOperationException("La contraseña actual es incorrecta.");

        // Hashear nueva contraseña
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.ContrasenaNueva);
        usuario.DebeCambiarContrasena = false;
        usuario.UltimoAcceso = DateTime.UtcNow;

        _usuarioRepository.Update(usuario);
        await _usuarioRepository.SaveChangesAsync();
    }
}