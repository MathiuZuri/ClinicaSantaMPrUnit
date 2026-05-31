using Clinica.API.Authorization;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.IntegrationTests.Helpers;

public static class TestDataSeeder
{
    public static async Task<Usuario> CrearUsuarioAsync(
        ApplicationDbContext db,
        string userName = "usuario_test",
        string correo = "usuario_test@clinica.com",
        string password = "test123",
        string nombres = "Usuario",
        string apellidos = "Prueba")
    {
        var usuario = new Usuario
        {
            CodigoUsuario = $"USR-TEST-{Guid.NewGuid():N}"[..18],
            Nombres = nombres,
            Apellidos = apellidos,
            UserName = userName,
            Correo = correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Estado = EstadoUsuario.Activo,
            FechaRegistro = DateTime.UtcNow
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        return usuario;
    }

    public static async Task<Rol> CrearRolAsync(
        ApplicationDbContext db,
        string nombre = "Rol Test",
        IEnumerable<string>? permisos = null)
    {
        var rol = new Rol
        {
            Nombre = $"{nombre} {Guid.NewGuid():N}"[..25],
            Descripcion = "Rol creado para pruebas de integración.",
            EsSistema = false,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        db.Roles.Add(rol);
        await db.SaveChangesAsync();

        if (permisos is not null)
        {
            await AsignarPermisosARolAsync(db, rol.Id, permisos);
        }

        return rol;
    }

    public static async Task AsignarRolAUsuarioAsync(
        ApplicationDbContext db,
        Guid usuarioId,
        Guid rolId)
    {
        var existe = await db.UsuarioRoles.AnyAsync(x =>
            x.UsuarioId == usuarioId &&
            x.RolId == rolId &&
            x.Activo);

        if (existe) return;

        db.UsuarioRoles.Add(new UsuarioRol
        {
            UsuarioId = usuarioId,
            RolId = rolId,
            FechaAsignacion = DateTime.UtcNow,
            Activo = true
        });

        await db.SaveChangesAsync();
    }

    public static async Task AsignarPermisosARolAsync(
        ApplicationDbContext db,
        Guid rolId,
        IEnumerable<string> codigosPermisos)
    {
        var codigos = codigosPermisos
            .Distinct()
            .ToList();

        var permisos = await db.Permisos
            .Where(x => codigos.Contains(x.Codigo))
            .ToListAsync();

        foreach (var codigo in codigos)
        {
            if (permisos.All(x => x.Codigo != codigo))
            {
                db.Permisos.Add(new Permiso
                {
                    Codigo = codigo,
                    Nombre = codigo,
                    Modulo = "Testing",
                    Activo = true
                });
            }
        }

        await db.SaveChangesAsync();

        permisos = await db.Permisos
            .Where(x => codigos.Contains(x.Codigo))
            .ToListAsync();

        var permisosActuales = await db.RolPermisos
            .Where(x => x.RolId == rolId)
            .Select(x => x.PermisoId)
            .ToListAsync();

        foreach (var permiso in permisos)
        {
            if (permisosActuales.Contains(permiso.Id)) continue;

            db.RolPermisos.Add(new RolPermiso
            {
                RolId = rolId,
                PermisoId = permiso.Id,
                FechaAsignacion = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    public static async Task<Usuario> CrearUsuarioConPermisosAsync(
        ApplicationDbContext db,
        string userName,
        string correo,
        IEnumerable<string> permisos,
        string password = "test123")
    {
        var usuario = await CrearUsuarioAsync(
            db,
            userName: userName,
            correo: correo,
            password: password
        );

        var rol = await CrearRolAsync(
            db,
            nombre: $"Rol {userName}",
            permisos: permisos
        );

        await AsignarRolAUsuarioAsync(db, usuario.Id, rol.Id);

        return usuario;
    }

    public static async Task<Usuario> CrearRecepcionistaAsync(
        ApplicationDbContext db,
        string userName = "recepcionista_test",
        string correo = "recepcionista_test@clinica.com",
        string password = "test123")
    {
        return await CrearUsuarioConPermisosAsync(
            db,
            userName,
            correo,
            new[]
            {
                PermisosPolicies.PacienteVer,
                PermisosPolicies.PacienteCrear,
                PermisosPolicies.PacienteEditar,
                PermisosPolicies.DoctorVer,
                PermisosPolicies.HorarioVer,
                PermisosPolicies.ServicioVer,
                PermisosPolicies.CitaVer,
                PermisosPolicies.CitaProgramar,
                PermisosPolicies.CitaReprogramar,
                PermisosPolicies.CitaCancelar
            },
            password
        );
    }

    public static async Task<Paciente> CrearPacienteAsync(
        ApplicationDbContext db,
        Guid? usuarioId = null,
        string dni = "12345678",
        string nombres = "Ana",
        string apellidos = "Quispe",
        string sexo = "F")
    {
        var usuario = usuarioId.HasValue
            ? null
            : await CrearUsuarioAsync(
                db,
                userName: $"paciente_{Guid.NewGuid():N}"[..18],
                correo: $"paciente_{Guid.NewGuid():N}@clinica.com",
                nombres: nombres,
                apellidos: apellidos
            );

        var paciente = new Paciente
        {
            CodigoPaciente = $"PAC-{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}"[..22],
            DNI = dni,
            Nombres = nombres,
            Apellidos = apellidos,
            FechaNacimiento = new DateTime(1998, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            Sexo = sexo,
            Celular = "987654321",
            Correo = $"paciente_{dni}@test.com",
            Direccion = "Jr. Lima 123",
            Estado = EstadoPaciente.Activo,
            UsuarioId = usuarioId ?? usuario!.Id,
            FechaRegistro = DateTime.UtcNow
        };

        db.Pacientes.Add(paciente);
        await db.SaveChangesAsync();

        return paciente;
    }

    public static async Task<Doctor> CrearDoctorAsync(
        ApplicationDbContext db,
        Guid? usuarioId = null,
        string cmp = "CMP12345",
        string nombres = "Maria",
        string apellidos = "Lopez",
        string especialidad = "Obstetricia")
    {
        var usuario = usuarioId.HasValue
            ? null
            : await CrearUsuarioAsync(
                db,
                userName: $"doctor_{Guid.NewGuid():N}"[..18],
                correo: $"doctor_{Guid.NewGuid():N}@clinica.com",
                nombres: nombres,
                apellidos: apellidos
            );

        var doctor = new Doctor
        {
            CodigoDoctor = $"DOC-{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}"[..22],
            CMP = cmp,
            Nombres = nombres,
            Apellidos = apellidos,
            Especialidad = especialidad,
            Celular = "987654321",
            Correo = $"doctor_{cmp}@test.com",
            FechaInicioContrato = DateTime.UtcNow.AddMonths(-1),
            FechaFinContrato = null,
            Estado = EstadoDoctor.Activo,
            UsuarioId = usuarioId ?? usuario!.Id
        };

        db.Doctores.Add(doctor);
        await db.SaveChangesAsync();

        return doctor;
    }

    public static async Task<ServicioClinico> ObtenerOCrearServicioClinicoAsync(
        ApplicationDbContext db,
        string codigoServicio = "CONOBS",
        string nombre = "Consulta obstétrica",
        decimal costoBase = 70,
        int duracionMinutos = 30)
    {
        var servicio = await db.ServiciosClinicos
            .FirstOrDefaultAsync(x => x.CodigoServicio == codigoServicio);

        if (servicio is not null)
            return servicio;

        servicio = new ServicioClinico
        {
            CodigoServicio = codigoServicio,
            Nombre = nombre,
            Descripcion = "Servicio creado para pruebas de integración.",
            CostoBase = costoBase,
            DuracionMinutos = duracionMinutos,
            RequiereCita = true,
            GeneraHistorial = true,
            Estado = EstadoServicioClinico.Activo
        };

        db.ServiciosClinicos.Add(servicio);
        await db.SaveChangesAsync();

        return servicio;
    }

    public static async Task<HorarioDoctor> CrearHorarioDoctorAsync(
        ApplicationDbContext db,
        Guid doctorId,
        DayOfWeek? diaSemana = null,
        TimeOnly? horaInicio = null,
        TimeOnly? horaFin = null)
    {
        var fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var horario = new HorarioDoctor
        {
            DoctorId = doctorId,
            DiaSemana = diaSemana ?? fecha.DayOfWeek,
            HoraInicio = horaInicio ?? new TimeOnly(8, 0),
            HoraFin = horaFin ?? new TimeOnly(12, 0),
            FechaInicioVigencia = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            FechaFinVigencia = null,
            Activo = true
        };

        db.HorariosDoctor.Add(horario);
        await db.SaveChangesAsync();

        return horario;
    }

    public static async Task<Cita> CrearCitaAsync(
        ApplicationDbContext db,
        Guid pacienteId,
        Guid doctorId,
        Guid servicioClinicoId,
        Guid? horarioDoctorId = null,
        DateOnly? fecha = null,
        TimeOnly? horaInicio = null,
        TimeOnly? horaFin = null,
        Guid? usuarioRegistroId = null,
        EstadoCita estado = EstadoCita.Pendiente)
    {
        var inicio = horaInicio ?? new TimeOnly(9, 0);
        var fin = horaFin ?? inicio.AddMinutes(30);

        var cita = new Cita
        {
            CodigoCita = $"CIT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..28],
            PacienteId = pacienteId,
            DoctorId = doctorId,
            ServicioClinicoId = servicioClinicoId,
            HorarioDoctorId = horarioDoctorId,
            Fecha = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            HoraInicio = inicio,
            HoraFin = fin,
            Motivo = "Cita creada para prueba de integración.",
            Observaciones = "Registro generado desde TestDataSeeder.",
            Estado = estado,
            FechaRegistro = DateTime.UtcNow,
            UsuarioRegistroId = usuarioRegistroId
        };

        db.Citas.Add(cita);
        await db.SaveChangesAsync();

        return cita;
    }

    public static async Task<(Paciente Paciente, Doctor Doctor, ServicioClinico Servicio, HorarioDoctor Horario)> CrearBaseParaCitaAsync(
        ApplicationDbContext db)
    {
        var paciente = await CrearPacienteAsync(
            db,
            dni: RandomDni()
        );

        var doctor = await CrearDoctorAsync(
            db,
            cmp: $"CMP{Random.Shared.Next(10000, 99999)}"
        );

        var servicio = await ObtenerOCrearServicioClinicoAsync(db);

        var horario = await CrearHorarioDoctorAsync(
            db,
            doctor.Id
        );

        return (paciente, doctor, servicio, horario);
    }

    public static async Task<TEntity> GuardarAsync<TEntity>(
        ApplicationDbContext db,
        TEntity entity)
        where TEntity : class
    {
        db.Set<TEntity>().Add(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    public static async Task<int> ContarAsync<TEntity>(
        ApplicationDbContext db)
        where TEntity : class
    {
        return await db.Set<TEntity>().CountAsync();
    }

    public static async Task<bool> ExisteAsync<TEntity>(
        ApplicationDbContext db,
        Guid id)
        where TEntity : class
    {
        var entity = await db.Set<TEntity>().FindAsync(id);
        return entity is not null;
    }
    
    public static async Task<HistorialClinico> CrearHistorialClinicoAsync(
    ApplicationDbContext db,
    Guid pacienteId,
    EstadoHistorialClinico estado = EstadoHistorialClinico.Activo)
{
    var historial = new HistorialClinico
    {
        CodigoHistorial = $"HC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..28],
        PacienteId = pacienteId,
        FechaApertura = DateTime.UtcNow,
        Estado = estado
    };

    db.HistorialesClinicos.Add(historial);
    await db.SaveChangesAsync();

    return historial;
}

    public static async Task<HistorialDetalle> CrearHistorialDetalleAsync(
        ApplicationDbContext db,
        Guid historialClinicoId,
        TipoMovimientoHistorial tipoMovimiento = TipoMovimientoHistorial.AperturaHistorial,
        string titulo = "Detalle de prueba",
        string descripcion = "Detalle creado para prueba de integración.",
        Guid? citaId = null,
        Guid? atencionId = null,
        Guid? pagoId = null,
        Guid? usuarioId = null)
    {
        var detalle = new HistorialDetalle
        {
            CodigoDetalle = $"HD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..28],
            HistorialClinicoId = historialClinicoId,
            TipoMovimiento = tipoMovimiento,
            CitaId = citaId,
            AtencionId = atencionId,
            PagoId = pagoId,
            Titulo = titulo,
            Descripcion = descripcion,
            FechaRegistro = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        db.HistorialDetalles.Add(detalle);
        await db.SaveChangesAsync();

        return detalle;
    }

    public static async Task<(Paciente Paciente, HistorialClinico Historial)> CrearPacienteConHistorialAsync(
        ApplicationDbContext db,
        string? dni = null)
    {
        var paciente = await CrearPacienteAsync(
            db,
            dni: dni ?? RandomDni()
        );

        var historial = await CrearHistorialClinicoAsync(
            db,
            paciente.Id
        );

        return (paciente, historial);
    }

    public static async Task<(Paciente Paciente, HistorialClinico Historial, HistorialDetalle Detalle)> CrearPacienteConHistorialYDetalleAsync(
        ApplicationDbContext db,
        string? dni = null)
    {
        var baseHistorial = await CrearPacienteConHistorialAsync(
            db,
            dni
        );

        var detalle = await CrearHistorialDetalleAsync(
            db,
            baseHistorial.Historial.Id,
            tipoMovimiento: TipoMovimientoHistorial.AperturaHistorial,
            titulo: "Apertura de historial clínico",
            descripcion: "Se apertura el historial clínico del paciente."
        );

        return (baseHistorial.Paciente, baseHistorial.Historial, detalle);
    }
    
    public static async Task<Atencion> CrearAtencionAsync(
    ApplicationDbContext db,
    Guid pacienteId,
    Guid doctorId,
    Guid servicioClinicoId,
    Guid historialClinicoId,
    Guid? citaId = null,
    decimal costoFinal = 100,
    decimal montoPagado = 0,
    string motivoConsulta = "Atención de prueba")
    {
    var atencion = new Atencion
    {
        CodigoAtencion = $"ATE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..28],
        PacienteId = pacienteId,
        DoctorId = doctorId,
        ServicioClinicoId = servicioClinicoId,
        CitaId = citaId,
        HistorialClinicoId = historialClinicoId,
        FechaInicio = DateTime.UtcNow,
        MotivoConsulta = motivoConsulta,
        Estado = EstadoAtencion.Abierta,
        CostoFinal = costoFinal,
        MontoPagado = montoPagado,
        SaldoPendiente = costoFinal - montoPagado
    };

    db.Atenciones.Add(atencion);
    await db.SaveChangesAsync();

    return atencion;
    }

    public static async Task<(Paciente Paciente, Doctor Doctor, ServicioClinico Servicio)> CrearBasePacienteDoctorServicioAsync(
    ApplicationDbContext db,
    string? dni = null,
    string? cmp = null)
    {
    var sufijo = Guid.NewGuid().ToString("N")[..8];

    var usuarioPaciente = new Usuario
    {
        CodigoUsuario = $"USR-{DateTime.UtcNow:yyyy}-PAC-{sufijo[..4]}",
        Nombres = "Paciente",
        Apellidos = "Finanzas",
        UserName = $"pac_fin_{sufijo}",
        Correo = $"pac_fin_{sufijo}@test.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
        Estado = EstadoUsuario.Activo,
        FechaRegistro = DateTime.UtcNow
    };

    var usuarioDoctor = new Usuario
    {
        CodigoUsuario = $"USR-{DateTime.UtcNow:yyyy}-DOC-{sufijo[..4]}",
        Nombres = "Doctor",
        Apellidos = "Finanzas",
        UserName = $"doc_fin_{sufijo}",
        Correo = $"doc_fin_{sufijo}@test.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
        Estado = EstadoUsuario.Activo,
        FechaRegistro = DateTime.UtcNow
    };

    db.Usuarios.AddRange(usuarioPaciente, usuarioDoctor);
    await db.SaveChangesAsync();

    var paciente = new Paciente
    {
        CodigoPaciente = $"PAC-{DateTime.UtcNow:yyyy}-{sufijo[..5].ToUpper()}",
        DNI = dni ?? Random.Shared.Next(10000000, 99999999).ToString(),
        Nombres = "Ana",
        Apellidos = "Quispe",
        FechaNacimiento = new DateTime(1998, 4, 15, 0, 0, 0, DateTimeKind.Utc),
        Sexo = "F",
        Celular = "987654321",
        Correo = $"ana_{sufijo}@test.com",
        Direccion = "Jr. Lima 123",
        Estado = EstadoPaciente.Activo,
        UsuarioId = usuarioPaciente.Id,
        FechaRegistro = DateTime.UtcNow
    };

    var doctor = new Doctor
    {
        CodigoDoctor = $"DOC-{DateTime.UtcNow:yyyy}-{sufijo[..5].ToUpper()}",
        CMP = cmp ?? $"CMP{sufijo[..6].ToUpper()}",
        Nombres = "María",
        Apellidos = "López",
        Especialidad = "Obstetricia",
        Celular = "987654322",
        Correo = $"doctor_{sufijo}@test.com",
        FechaInicioContrato = DateTime.UtcNow.Date,
        Estado = EstadoDoctor.Activo,
        UsuarioId = usuarioDoctor.Id
    };

    db.Pacientes.Add(paciente);
    db.Doctores.Add(doctor);
    await db.SaveChangesAsync();

    var servicio = await db.ServiciosClinicos.FirstAsync();

    return (paciente, doctor, servicio);
    }

    public static async Task<AjusteFinanciero> CrearAjusteFinancieroAsync(
        ApplicationDbContext db,
        Guid pagoId,
        Guid pacienteId,
        Guid? atencionId = null,
        TipoAjusteFinanciero tipoAjuste = TipoAjusteFinanciero.Descuento,
        decimal montoAjuste = 10,
        string motivo = "Ajuste financiero de prueba",
        string? observacion = "Observación de prueba",
        Guid? usuarioRegistroId = null,
        DateTime? fechaRegistro = null)
    {
        var ajuste = new AjusteFinanciero
        {
            PagoId = pagoId,
            PacienteId = pacienteId,
            AtencionId = atencionId,
            TipoAjuste = tipoAjuste,
            MontoAjuste = montoAjuste,
            Motivo = motivo,
            Observacion = observacion,
            UsuarioRegistroId = usuarioRegistroId,
            FechaRegistro = fechaRegistro ?? DateTime.UtcNow
        };

        db.AjustesFinancieros.Add(ajuste);
        await db.SaveChangesAsync();

        return ajuste;
    }
    
    public static async Task<Pago> CrearPagoAsync(
        ApplicationDbContext db,
        Guid pacienteId,
        Guid servicioClinicoId,
        Guid? citaId = null,
        Guid? atencionId = null,
        decimal montoTotal = 100,
        decimal montoPagado = 100,
        decimal montoAdelanto = 0,
        MetodoPago metodoPago = MetodoPago.Efectivo,
        EstadoPago? estado = null,
        string? observacion = "Pago creado para prueba de integración.",
        Guid? usuarioRegistroId = null,
        DateTime? fechaPago = null)
    {
        var saldoPendiente = montoTotal - montoPagado;

        var pago = new Pago
        {
            CodigoPago = $"PAG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..28],
            PacienteId = pacienteId,
            ServicioClinicoId = servicioClinicoId,
            CitaId = citaId,
            AtencionId = atencionId,
            MontoTotal = montoTotal,
            MontoPagado = montoPagado,
            SaldoPendiente = saldoPendiente,
            MontoAdelanto = montoAdelanto,
            MetodoPago = metodoPago,
            Estado = estado ?? (saldoPendiente == 0 ? EstadoPago.Pagado : EstadoPago.Parcial),
            Observacion = observacion,
            FechaPago = fechaPago ?? DateTime.UtcNow,
            UsuarioRegistroId = usuarioRegistroId
        };

        db.Pagos.Add(pago);
        await db.SaveChangesAsync();

        return pago;
    }
    
    private static string RandomDni()
    {
        return Random.Shared.Next(10000000, 99999999).ToString();
    }
}