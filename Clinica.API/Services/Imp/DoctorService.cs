using BCrypt.Net; // <-- mismo using que en UsuarioService
using Clinica.API.Helpers;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.DTOs.Doctores;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;

namespace Clinica.API.Services.Imp;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IUsuarioActualService usuarioActualService,
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPermisoRepository permisoRepository)
    {
        _doctorRepository = doctorRepository;
        _usuarioActualService = usuarioActualService;
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
    }

    public async Task<IEnumerable<DoctorResponseDto>> ObtenerTodosAsync()
    {
        var doctores = await _doctorRepository.GetAllAsync();
        return doctores.Select(MapearDoctor);
    }

    public async Task<IEnumerable<DoctorResponseDto>> ObtenerActivosAsync()
    {
        var doctores = await _doctorRepository.ObtenerActivosAsync();
        return doctores.Select(MapearDoctor);
    }

    public async Task<DoctorResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null) return null;
        return MapearDoctor(doctor);
    }

    public async Task<Guid> CrearAsync(CrearDoctorDto dto)
    {
        var existe = await _doctorRepository.ObtenerPorCmpAsync(dto.CMP);
        if (existe != null)
            throw new InvalidOperationException("Ya existe un doctor registrado con ese CMP.");

        if (dto.FechaFinContrato.HasValue && dto.FechaFinContrato.Value < dto.FechaInicioContrato)
            throw new InvalidOperationException("La fecha de fin de contrato no puede ser menor que la fecha de inicio.");

        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            CodigoDoctor = GenerarCodigoDoctor(dto.CMP),
            CMP = dto.CMP,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Especialidad = dto.Especialidad,
            Celular = dto.Celular,
            Correo = dto.Correo,
            FechaInicioContrato = FechaHelper.ToUtc(dto.FechaInicioContrato),
            FechaFinContrato = FechaHelper.ToUtc(dto.FechaFinContrato),
            UsuarioId = usuarioId,
            Estado = EstadoDoctor.Activo
        };

        await _doctorRepository.AddAsync(doctor);
        await _doctorRepository.SaveChangesAsync();

        return doctor.Id;
    }

    public async Task ActualizarAsync(Guid id, EditarDoctorDto dto)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null)
            throw new KeyNotFoundException("Doctor no encontrado.");

        if (dto.FechaFinContrato.HasValue && dto.FechaFinContrato.Value < dto.FechaInicioContrato)
            throw new InvalidOperationException("La fecha de fin de contrato no puede ser menor que la fecha de inicio.");

        doctor.CMP = dto.CMP;
        doctor.Nombres = dto.Nombres;
        doctor.Apellidos = dto.Apellidos;
        doctor.Especialidad = dto.Especialidad;
        doctor.Celular = dto.Celular;
        doctor.Correo = dto.Correo;
        doctor.FechaInicioContrato = FechaHelper.ToUtc(dto.FechaInicioContrato);
        doctor.FechaFinContrato = FechaHelper.ToUtc(dto.FechaFinContrato);
        doctor.Estado = dto.Estado;

        _doctorRepository.Update(doctor);
        await _doctorRepository.SaveChangesAsync();
    }

    // ============= NUEVO: CONTRATAR MÉDICO =============
    public async Task<Guid> ContratarAsync(ContratarDoctorDto dto)
    {
        // Validar que el CMP no exista
        var existeDoctor = await _doctorRepository.ObtenerPorCmpAsync(dto.CMP);
        if (existeDoctor != null)
            throw new InvalidOperationException("Ya existe un doctor con ese CMP.");

        // Validar que el nombre de usuario y correo no existan
        var existeUsuario = await _usuarioRepository.ObtenerPorUserNameAsync(dto.UserName);
        if (existeUsuario != null)
            throw new InvalidOperationException("El nombre de usuario ya está en uso.");

        var existeCorreo = await _usuarioRepository.ObtenerPorCorreoAsync(dto.CorreoUsuario);
        if (existeCorreo != null)
            throw new InvalidOperationException("El correo ya está registrado.");

        // Crear el usuario (mismo estilo que en UsuarioService)
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            CodigoUsuario = GenerarCodigoUsuario(),
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            UserName = dto.UserName,
            Correo = dto.CorreoUsuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // <-- igual que en UsuarioService
            FechaRegistro = DateTime.UtcNow,
            Estado = EstadoUsuario.Activo,
            DebeCambiarContrasena = true
        };

        await _usuarioRepository.AddAsync(usuario);

        // Asignar rol (si se envía RolId, usarlo; sino, buscar rol "Doctor")
        Guid rolId;
        if (dto.RolId.HasValue)
        {
            var rol = await _rolRepository.GetByIdAsync(dto.RolId.Value);
            if (rol == null)
                throw new KeyNotFoundException("El rol especificado no existe.");
            rolId = rol.Id;
        }
        else
        {
            var rolDoctor = await _rolRepository.ObtenerPorNombreAsync("Doctor");
            if (rolDoctor == null)
                throw new InvalidOperationException("El rol 'Doctor' no existe en el sistema. Ejecute el seeder.");
            rolId = rolDoctor.Id;
        }

        // Verificar si ya tiene el rol
        var yaTieneRol = await _usuarioRepository.TieneRolAsignadoAsync(usuario.Id, rolId);
        if (!yaTieneRol)
        {
            await _usuarioRepository.AgregarRolAsync(new UsuarioRol
            {
                UsuarioId = usuario.Id,
                RolId = rolId,
                FechaAsignacion = DateTime.UtcNow,
                Activo = true
            });
        }

        // Si se enviaron permisos adicionales, asignarlos al rol
        if (dto.PermisosIds != null && dto.PermisosIds.Any())
        {
            foreach (var permisoId in dto.PermisosIds.Distinct())
            {
                var permiso = await _permisoRepository.GetByIdAsync(permisoId);
                if (permiso == null) continue;
                var yaTienePermiso = await _rolRepository.TienePermisoAsignadoAsync(rolId, permisoId);
                if (!yaTienePermiso)
                {
                    await _rolRepository.AgregarPermisoAsync(new RolPermiso
                    {
                        RolId = rolId,
                        PermisoId = permisoId,
                        FechaAsignacion = DateTime.UtcNow
                    });
                }
            }
        }

        // Crear el doctor asociado al usuario
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            CodigoDoctor = GenerarCodigoDoctor(dto.CMP),
            CMP = dto.CMP,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Especialidad = dto.Especialidad,
            Celular = dto.Celular,
            Correo = dto.Correo,
            FechaInicioContrato = FechaHelper.ToUtc(dto.FechaInicioContrato),
            FechaFinContrato = FechaHelper.ToUtc(dto.FechaFinContrato),
            UsuarioId = usuario.Id,
            Estado = EstadoDoctor.Activo
        };

        await _doctorRepository.AddAsync(doctor);
        await _doctorRepository.SaveChangesAsync();

        return doctor.Id;
    }

    // ============= NUEVO: BÚSQUEDA CON PAGINACIÓN =============
    public async Task<PaginacionResponseDto<DoctorResponseDto>> BuscarAsync(
        string? nombre,
        string? especialidad,
        EstadoDoctor? estado,
        PaginacionRequestDto request)
    {
        var doctores = await _doctorRepository.GetAllAsync();

        var query = doctores.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            nombre = nombre.Trim().ToLower();
            query = query.Where(d =>
                d.Nombres.ToLower().Contains(nombre) ||
                d.Apellidos.ToLower().Contains(nombre));
        }

        if (!string.IsNullOrWhiteSpace(especialidad))
            query = query.Where(d => d.Especialidad.ToLower().Contains(especialidad.Trim().ToLower()));

        if (estado.HasValue)
            query = query.Where(d => d.Estado == estado.Value);

        var total = query.Count();
        var items = query
            .OrderBy(d => d.Nombres)
            .Skip((request.Pagina - 1) * request.CantidadPorPagina)
            .Take(request.CantidadPorPagina)
            .Select(MapearDoctor)
            .ToList();

        return new PaginacionResponseDto<DoctorResponseDto>
        {
            Pagina = request.Pagina,
            CantidadPorPagina = request.CantidadPorPagina,
            TotalRegistros = total,
            Datos = items
        };
    }

    // ============= MÉTODOS PRIVADOS =============
    private static DoctorResponseDto MapearDoctor(Doctor doctor)
    {
        return new DoctorResponseDto
        {
            Id = doctor.Id,
            CodigoDoctor = doctor.CodigoDoctor,
            CMP = doctor.CMP,
            Nombres = doctor.Nombres,
            Apellidos = doctor.Apellidos,
            Especialidad = doctor.Especialidad,
            Celular = doctor.Celular,
            Correo = doctor.Correo,
            FechaInicioContrato = doctor.FechaInicioContrato,
            FechaFinContrato = doctor.FechaFinContrato,
            Estado = doctor.Estado
        };
    }

    private static string GenerarCodigoDoctor(string cmp)
    {
        return $"DOC-{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{cmp}";
    }

    private static string GenerarCodigoUsuario()
    {
        return $"USR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..5].ToUpper()}";
    }
}