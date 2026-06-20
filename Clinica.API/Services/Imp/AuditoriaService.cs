using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clinica.Domain.DTOs.Auditoria;
using Clinica.Domain.DTOs.Comunes;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Services.Imp;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly ApplicationDbContext _context;

    public AuditoriaService(IAuditoriaRepository auditoriaRepository, ApplicationDbContext context)
    {
        _auditoriaRepository = auditoriaRepository;
        _context = context;
    }

    // ✅ MEJORA: Usamos Expression para que Entity Framework Core pueda traducirlo a SQL sin dar error.
    private static Expression<Func<Auditoria, AuditoriaResponseDto>> SelectorDto => x => new AuditoriaResponseDto
    {
        Id = x.Id,
        UsuarioId = x.UsuarioId,
        UsuarioNombre = x.Usuario == null ? null : $"{x.Usuario.Nombres} {x.Usuario.Apellidos}",
        TipoAccion = x.TipoAccion,
        Modulo = x.Modulo,
        EntidadAfectada = x.EntidadAfectada,
        EntidadId = x.EntidadId,
        Descripcion = x.Descripcion,
        ValorAnterior = x.ValorAnterior,
        ValorNuevo = x.ValorNuevo,
        IpAddress = x.IpAddress,
        UserAgent = x.UserAgent,
        FueExitoso = x.FueExitoso,
        DetalleError = x.DetalleError,
        Nivel = x.Nivel,
        FechaHora = x.FechaHora,
        EsConsulta = x.EsConsulta
    };

    // ========== MÉTODOS ORIGINALES (SIN PAGINAR) ==========
    public async Task<IEnumerable<AuditoriaResponseDto>> ObtenerTodosAsync()
    {
        var auditorias = await _auditoriaRepository.GetAllAsync();

        // Usamos .Compile() porque aquí estamos trabajando con IEnumerable (memoria)
        return auditorias.Select(SelectorDto.Compile());
    }

    public async Task<IEnumerable<AuditoriaResponseDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
    {
        var auditorias = await _auditoriaRepository.ObtenerPorUsuarioAsync(usuarioId);

        // Usamos .Compile() porque aquí estamos trabajando con IEnumerable (memoria)
        return auditorias.Select(SelectorDto.Compile());
    }

    // ========== NUEVOS MÉTODOS CON PAGINACIÓN Y FILTROS ==========

    public async Task<PaginacionResponseDto<AuditoriaResponseDto>> ObtenerTodosPaginadosAsync(
        PaginacionRequestDto request,
        TipoAccionAuditoria? tipoAccion = null,
        bool? soloConsultas = null)
    {
        // ✅ 1. Declaración limpia como IQueryable (Sin OrderBy todavía)
        IQueryable<Auditoria> query = _context.Auditorias
            .Include(x => x.Usuario)
            .AsNoTracking();

        // ✅ 2. Aplicar filtros dinámicos
        if (tipoAccion.HasValue)
            query = query.Where(x => x.TipoAccion == tipoAccion.Value);

        if (soloConsultas.HasValue)
            query = query.Where(x => x.EsConsulta == soloConsultas.Value);

        // ✅ 3. Aplicar OrderBy al final para no romper el tipo IQueryable
        query = query.OrderByDescending(x => x.FechaHora);

        var total = await query.CountAsync();

        // ✅ 4. Paginación y Mapeo usando la Expresión centralizada
        var items = await query
            .Skip((request.Pagina - 1) * request.CantidadPorPagina)
            .Take(request.CantidadPorPagina)
            .Select(SelectorDto) // <-- EF Core traduce esto perfecto a SQL
            .ToListAsync();

        return new PaginacionResponseDto<AuditoriaResponseDto>
        {
            Pagina = request.Pagina,
            CantidadPorPagina = request.CantidadPorPagina,
            TotalRegistros = total,
            Datos = items
        };
    }

    public async Task<PaginacionResponseDto<AuditoriaResponseDto>> ObtenerPorUsuarioPaginadosAsync(
        Guid usuarioId,
        PaginacionRequestDto request,
        TipoAccionAuditoria? tipoAccion = null,
        bool? soloConsultas = null)
    {
        // ✅ 1. Declaración limpia como IQueryable (Filtrando por el Usuario directamente)
        IQueryable<Auditoria> query = _context.Auditorias
            .Include(x => x.Usuario)
            .Where(x => x.UsuarioId == usuarioId)
            .AsNoTracking();

        // ✅ 2. Aplicar filtros dinámicos adicionales
        if (tipoAccion.HasValue)
            query = query.Where(x => x.TipoAccion == tipoAccion.Value);

        if (soloConsultas.HasValue)
            query = query.Where(x => x.EsConsulta == soloConsultas.Value);

        // ✅ 3. Aplicar OrderBy al final
        query = query.OrderByDescending(x => x.FechaHora);

        var total = await query.CountAsync();

        // ✅ 4. Paginación y Mapeo
        var items = await query
            .Skip((request.Pagina - 1) * request.CantidadPorPagina)
            .Take(request.CantidadPorPagina)
            .Select(SelectorDto)
            .ToListAsync();

        return new PaginacionResponseDto<AuditoriaResponseDto>
        {
            Pagina = request.Pagina,
            CantidadPorPagina = request.CantidadPorPagina,
            TotalRegistros = total,
            Datos = items
        };
    }
}