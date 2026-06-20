using Clinica.Domain.DTOs.Pacientes;
using Clinica.Domain.Entities;
using Clinica.Domain.Interfaces;
using Clinica.API.Helpers;
using Clinica.Domain.Enums;
using Microsoft.EntityFrameworkCore; // Necesario para FirstOrDefaultAsync

namespace Clinica.API.Services.Imp;

public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IHistorialClinicoRepository _historialRepository;
    private readonly IHistorialDetalleRepository _historialDetalleRepository;
    private readonly IUsuarioActualService _usuarioActualService;
    
    public PacienteService(
        IPacienteRepository pacienteRepository,
        IHistorialClinicoRepository historialRepository,
        IHistorialDetalleRepository historialDetalleRepository,
        IUsuarioActualService usuarioActualService)
    {
        _pacienteRepository = pacienteRepository;
        _historialRepository = historialRepository;
        _historialDetalleRepository = historialDetalleRepository;
        _usuarioActualService = usuarioActualService;
    }

    public async Task<IEnumerable<PacienteResponseDto>> ObtenerTodosAsync()
    {
        var pacientes = await _pacienteRepository.GetAllAsync();

        return pacientes.Select(x => new PacienteResponseDto
        {
            Id = x.Id,
            CodigoPaciente = x.CodigoPaciente,
            DNI = x.DNI,
            Nombres = x.Nombres,
            Apellidos = x.Apellidos,
            FechaNacimiento = x.FechaNacimiento,
            Sexo = x.Sexo,
            Celular = x.Celular,
            Correo = x.Correo,
            Direccion = x.Direccion,
            
            // Filiacion
            LugarNacimiento = x.LugarNacimiento,
            GradoInstruccion = x.GradoInstruccion,
            Ocupacion = x.Ocupacion,
            Religion = x.Religion,
            EstadoCivil = x.EstadoCivil,
            NombrePareja = x.NombrePareja,
            CelularPareja = x.CelularPareja,

            Estado = x.Estado,
            FechaRegistro = x.FechaRegistro,
            CodigoHistorial = x.HistorialClinico?.CodigoHistorial
        });
    }

    public async Task<PacienteResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        var paciente = await _pacienteRepository.ObtenerConHistorialAsync(id);
        if (paciente == null) return null;

        return MapearPaciente(paciente);
    }

    public async Task<PacienteResponseDto?> ObtenerPorDniAsync(string dni)
    {
        var paciente = await _pacienteRepository.ObtenerPorDniAsync(dni);
        if (paciente == null) return null;

        return MapearPaciente(paciente);
    }

    public async Task<Guid> CrearAsync(CrearPacienteDto dto)
    {
        var usuarioId = _usuarioActualService.ObtenerUsuarioId();

        var existe = await _pacienteRepository.ObtenerPorDniAsync(dto.DNI);
        if (existe != null)
            throw new InvalidOperationException("Ya existe un paciente registrado con ese DNI.");

        var paciente = new Paciente
        {
            Id = Guid.NewGuid(),
            CodigoPaciente = GenerarCodigoPaciente(dto.DNI),
            DNI = dto.DNI,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            FechaNacimiento = FechaHelper.ToUtc(dto.FechaNacimiento),
            Sexo = dto.Sexo,
            Celular = dto.Celular,
            Correo = dto.Correo,
            Direccion = dto.Direccion,
            
            // Filiación
            LugarNacimiento = dto.LugarNacimiento,
            GradoInstruccion = dto.GradoInstruccion,
            Ocupacion = dto.Ocupacion,
            Religion = dto.Religion,
            EstadoCivil = dto.EstadoCivil,
            NombrePareja = dto.NombrePareja,
            CelularPareja = dto.CelularPareja,

            UsuarioId = usuarioId,
            FechaRegistro = DateTime.UtcNow
        };

        // LLAMADA AL NUEVO GENERADOR CORRELATIVO
        var codigoHistorial = await GenerarCodigoHistorialAsync();

        var historial = new HistorialClinico
        {
            Id = Guid.NewGuid(),
            CodigoHistorial = codigoHistorial, // Asignación del nuevo código "FIL-yyyy-####"
            PacienteId = paciente.Id,
            FechaApertura = DateTime.UtcNow
        };

        var detalle = new HistorialDetalle
        {
            Id = Guid.NewGuid(),
            CodigoDetalle = GenerarCodigoDetalle("REG", dto.DNI),
            HistorialClinicoId = historial.Id,
            TipoMovimiento = Clinica.Domain.Enums.TipoMovimientoHistorial.AperturaHistorial,
            Titulo = "Apertura de historial clínico",
            Descripcion = "Se registró al paciente y se aperturó su historial clínico.",
            FechaRegistro = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _pacienteRepository.AddAsync(paciente);
        await _historialRepository.AddAsync(historial);
        await _historialDetalleRepository.AddAsync(detalle);

        await _pacienteRepository.SaveChangesAsync();

        return paciente.Id;
    }

    public async Task ActualizarContactoAsync(Guid id, ActualizarContactoPacienteDto dto)
    {
        var paciente = await _pacienteRepository.GetByIdAsync(id);
        if (paciente == null)
            throw new KeyNotFoundException("Paciente no encontrado.");

        paciente.Celular = dto.Celular;
        paciente.Correo = dto.Correo;
        paciente.Direccion = dto.Direccion;

        _pacienteRepository.Update(paciente);
        await _pacienteRepository.SaveChangesAsync();
    }

    private static PacienteResponseDto MapearPaciente(Paciente paciente)
    {
        return new PacienteResponseDto
        {
            Id = paciente.Id,
            CodigoPaciente = paciente.CodigoPaciente,
            DNI = paciente.DNI,
            Nombres = paciente.Nombres,
            Apellidos = paciente.Apellidos,
            FechaNacimiento = paciente.FechaNacimiento,
            Sexo = paciente.Sexo,
            Celular = paciente.Celular,
            Correo = paciente.Correo,
            Direccion = paciente.Direccion,
            
            // Filiacion
            LugarNacimiento = paciente.LugarNacimiento,
            GradoInstruccion = paciente.GradoInstruccion,
            Ocupacion = paciente.Ocupacion,
            Religion = paciente.Religion,
            EstadoCivil = paciente.EstadoCivil,
            NombrePareja = paciente.NombrePareja,
            CelularPareja = paciente.CelularPareja,

            Estado = paciente.Estado,
            FechaRegistro = paciente.FechaRegistro,
            CodigoHistorial = paciente.HistorialClinico?.CodigoHistorial
        };
    }
    
    public async Task CambiarEstadoAsync(Guid id, CambiarEstadoPacienteDto dto)
    {
        var paciente = await _pacienteRepository.GetByIdAsync(id);
        if (paciente == null)
            throw new KeyNotFoundException("Paciente no encontrado.");
        
        if (paciente.Estado == EstadoPaciente.Eliminado)
            throw new InvalidOperationException("No se puede cambiar el estado de un paciente eliminado.");

        paciente.Estado = dto.Estado;
        _pacienteRepository.Update(paciente);
        await _pacienteRepository.SaveChangesAsync();
    }

    private static string GenerarCodigoPaciente(string dni)
    {
        return $"PCT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{dni}";
    }
    
    // NUEVA LÓGICA: GENERADOR CORRELATIVO "FIL-año-0001"
    private async Task<string> GenerarCodigoHistorialAsync()
    {
        var anioActual = DateTime.UtcNow.Year.ToString(); // "2026"
        var prefijo = $"FIL-{anioActual}-";

        // 1. Usamos GetAllAsync() que sí existe en tu repositorio
        var historiales = await _historialRepository.GetAllAsync();

        // 2. Filtramos con LINQ estándar (usamos FirstOrDefault normal, sin "Async")
        var ultimoHistorial = historiales
            .Where(h => !string.IsNullOrEmpty(h.CodigoHistorial) && h.CodigoHistorial.StartsWith(prefijo))
            .OrderByDescending(h => h.CodigoHistorial)
            .FirstOrDefault();

        if (ultimoHistorial == null)
        {
            return $"{prefijo}0001"; // El primero del año
        }

        // Dividimos "FIL-2026-0005" y extraemos "0005"
        var partes = ultimoHistorial.CodigoHistorial.Split('-');
        if (partes.Length == 3 && int.TryParse(partes[2], out int ultimoCorrelativo))
        {
            int nuevoCorrelativo = ultimoCorrelativo + 1;
            return $"{prefijo}{nuevoCorrelativo:D4}"; // Aplica formato de 4 ceros (0006)
        }

        // Si ocurre un error de parseo (datos antiguos con otro formato), reinicia
        return $"{prefijo}0001";
    }

    private static string GenerarCodigoDetalle(string codigoServicio, string dni)
    {
        return $"{Guid.NewGuid().ToString("N")[..5].ToUpper()}-{codigoServicio}-{DateTime.UtcNow:yyyy}-{dni}";
    }
}