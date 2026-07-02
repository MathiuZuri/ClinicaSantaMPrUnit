# ⚙️ Capa de Infraestructura (Persistencia y Adaptadores) - SYS Clínica Santa Mónica

La **Capa de Infraestructura** constituye el **adaptador de salida** (*Output Adapter*) de la Arquitectura Hexagonal del sistema **SYS Clínica Santa Mónica**. Su responsabilidad fundamental es proporcionar implementaciones concretas y tecnológicas a los **puertos de salida** (interfaces de repositorios y servicios) declarados en las capas internas del sistema. 

Esta capa interactúa directamente con agentes e infraestructuras externas, como el motor relacional PostgreSQL en la nube de Neon.tech, los sistemas de archivos locales para la generación de reportes binarios PDF con QuestPDF, y las pasarelas criptográficas de hashing. Gracias al principio de inversión de dependencias, el impacto de estas tecnologías externas se mantiene completamente aislado del núcleo del negocio ginecobstétrico.

!!! info "Frontera Arquitectónica y Mitigación de Riesgos Regionales"
    - **Dependencias PERMITIDAS:** Entity Framework Core 9, Npgsql (PostgreSQL Native Driver), QuestPDF (motores de maquetación de documentos planos), BCrypt.Net (algoritmos de hashing de contraseñas) y el proyecto central `Clinica.Domain`.
    - **Dependencias PROHIBIDAS:** Referencias directas a la capa de API o de Presentación (Blazor WASM). No puede influenciar la lógica de orquestación de comandos ni las vistas del cliente.
    - **Resiliencia ante Entornos Inestables (Juliaca):** La infraestructura está diseñada de forma transaccional y elástica. El uso de repositorios asíncronos y un clúster serverless mitiga el riesgo de corrupción de datos clínicos e históricos ante los cortes imprevistos de energía o caídas de conectividad de red característicos del altiplano peruano.

---

## 🗄️ Contexto de Base de Datos (ApplicationDbContext)

El `ApplicationDbContext` actúa como la puerta de acceso centralizada al almacenamiento persistente. Hereda de `DbContext` de Entity Framework Core 9 y expone cada entidad del dominio mediante propiedades de tipo `DbSet<T>`. Su configuración interna escanea el ensamblado de manera automática para inyectar las reglas de la Fluent API sin ensuciar la declaración de las colecciones.

```csharp
using Microsoft.EntityFrameworkCore;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Entities.WHATSAPP;

namespace Clinica.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ====== CAPA DE SEGURIDAD Y CONTROL DE ACCESO (RBAC) ======
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    // ====== CAPA OPERATIVA Y ENTIDADES CLÍNICAS ======
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Doctor> Doctores => Set<Doctor>();
    public DbSet<HorarioDoctor> HorariosDoctor => Set<HorarioDoctor>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<ServicioClinico> ServiciosClinicos => Set<ServicioClinico>();
    public DbSet<HistorialClinico> HistorialesClinicos => Set<HistorialClinico>();
    public DbSet<HistorialDetalle> HistorialDetalles => Set<HistorialDetalle>();
    public DbSet<Atencion> Atenciones => Set<Atencion>();

    // ====== CAPA FINANCIERA, CAJA Y RECAUDACIÓN ======
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<AjusteFinanciero> AjustesFinancieros => Set<AjusteFinanciero>();
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<ComprobanteDetalle> ComprobanteDetalles => Set<ComprobanteDetalle>();
    
    // ====== SUBMÓDULOS CLÍNICOS EVOLUTIVOS (OBSTETRICIA) ======
    public DbSet<Anamnesis> Anamnesis => Set<Anamnesis>();
    public DbSet<ExamenFisico> ExamenesFisicos => Set<ExamenFisico>();
    public DbSet<TactoVaginal> TactosVaginales => Set<TactoVaginal>();
    public DbSet<EcografiaObstetrica> EcografiasObstetricas => Set<EcografiaObstetrica>();
    public DbSet<ImpresionDiagnostica> ImpresionesDiagnosticas => Set<ImpresionDiagnostica>();
    
    // ====== MÓDULOS DE COMUNICACIÓN ASÍNCRONA (EVOLUTION API) ======
    public DbSet<NotificacionCita> NotificacionesCitas => Set<NotificacionCita>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<MensajeChat> MensajesChat => Set<MensajeChat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Escaneo y mapeo automático de clases IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

###📐 Configuraciones de Entidades (Entity Configurations)
Para cumplir rigurosamente con los marcos regulatorios de SUSALUD, MINSA y SUNAT, las clases de configuración de la Fluent API encapsulan restricciones explícitas de tipos de datos, índices únicos de auditoría y comportamientos estrictos de integridad referencial.

=== "AtencionConfiguration"
```csharp
public class AtencionConfiguration : IEntityTypeConfiguration
{
public void Configure(EntityTypeBuilder builder)
{
builder.ToTable("Atenciones");
builder.HasKey(x => x.Id);

        builder.Property(x => x.CodigoAtencion).IsRequired().HasMaxLength(30);
        builder.HasIndex(x => x.CodigoAtencion).IsUnique();
        builder.Property(x => x.Estado).HasConversion<string>().IsRequired().HasMaxLength(30);
        
        // Relaciones de Control e Identidad de Personal
        builder.HasOne(x => x.Paciente).WithMany(p => p.Atenciones).HasForeignKey(x => x.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Doctor).WithMany(d => d.Atenciones).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Cita).WithOne(c => c.Atencion).HasForeignKey<Atencion>(x => x.CitaId).OnDelete(DeleteBehavior.SetNull);
        
        // Ciclo de Vida de Submódulos Clínicos (Eliminación Atada al Agregado Raíz)
        builder.HasOne(a => a.Anamnesis).WithOne(an => an.Atencion).HasForeignKey<Anamnesis>(an => an.AtencionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.ImpresionDiagnostica).WithOne(id => id.Atencion).HasForeignKey<ImpresionDiagnostica>(id => id.AtencionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.ExamenesFisicos).WithOne(ef => ef.Atencion).HasForeignKey(ef => ef.AtencionId).OnDelete(DeleteBehavior.Cascade);
    }
}
```
=== "ComprobanteConfiguration"
```csharp
public class ComprobanteConfiguration : IEntityTypeConfiguration
{
public void Configure(EntityTypeBuilder builder)
{
builder.ToTable("Comprobantes");
builder.HasKey(x => x.Id);

        builder.Property(x => x.CodigoComprobante).IsRequired().HasMaxLength(60);
        builder.HasIndex(x => x.CodigoComprobante).IsUnique();
        builder.Property(x => x.Serie).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Numero).IsRequired();
        builder.HasIndex(x => new { x.Serie, x.Numero, x.TipoComprobante }).IsUnique();
        
        builder.Property(x => x.TipoComprobante).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(x => x.Estado).HasConversion<string>().IsRequired().HasMaxLength(40);
        
        // Cumplimiento SUNAT: Snapshot Inmutable mediante tipo binario nativo JSONB de Postgres
        builder.Property(x => x.DatosSnapshotJson).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        
        builder.HasOne(x => x.Paciente).WithMany(x => x.Comprobantes).HasForeignKey(x => x.PacienteId).OnDelete(DeleteBehavior.Restrict);
    }
}
```
=== "UsuarioConfiguration"
```csharp
public class UsuarioConfiguration : IEntityTypeConfiguration
{
public void Configure(EntityTypeBuilder builder)
{
builder.ToTable("Usuarios");
builder.HasKey(x => x.Id);

        builder.Property(x => x.UserName).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.Property(x => x.Correo).IsRequired().HasMaxLength(150);
        builder.HasIndex(x => x.Correo).IsUnique();
        
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Estado).HasConversion<string>().IsRequired().HasMaxLength(30);
        
        builder.HasMany(x => x.UsuarioRoles).WithOne(x => x.Usuario).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
```
=== "PacienteConfiguration"
```csharp
public class PacienteConfiguration : IEntityTypeConfiguration
{
public void Configure(EntityTypeBuilder builder)
{
builder.ToTable("Pacientes");
builder.HasKey(x => x.Id);

        builder.Property(x => x.DNI).IsRequired().HasMaxLength(11);
        builder.HasIndex(x => x.DNI).IsUnique();
        builder.Property(x => x.Nombres).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Apellidos).IsRequired().HasMaxLength(100);
        
        // Módulo de Filiación Avanzada (Especificación Demográfica Regional)
        builder.Property(x => x.LugarNacimiento).HasMaxLength(150);
        builder.Property(x => x.GradoInstruccion).HasMaxLength(100);
        builder.Property(x => x.Ocupacion).HasMaxLength(150);
        
        builder.HasOne(x => x.HistorialClinico).WithOne(x => x.Paciente).HasForeignKey<HistorialClinico>(x => x.PacienteId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

## 🗂️ Repositorios (Implementación de Puertos de Salida)

El acceso a los datos implementa el patrón Repository. El `GenericRepository<T>` resuelve los comandos CRUD comunes y centraliza las llamadas de confirmación atómica (`SaveChangesAsync`).

### Adaptador Genérico Base

```csharp
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ApplicationDbContext context) 
    { 
        Context = context; 
        DbSet = context.Set<T>(); 
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await DbSet.ToListAsync();
    public async Task<T?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);
    public async Task AddAsync(T entity) { await DbSet.AddAsync(entity); await Context.SaveChangesAsync(); }
    public void Update(T entity) => DbSet.Update(entity);
    public void Delete(T entity) => DbSet.Remove(entity);
    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
```

### Adaptadores Especializados (Eager Loading Estricto)
Para contrarrestar las latencias físicas de red y optimizar las transacciones concurrentes en Juliaca, los repositorios específicos ejecutan técnicas de Eager Loading coordinado. Esto consolida grafos complejos de datos en una única consulta indexada en la base de datos de Neon:

=== "AtencionRepository"

```csharp
public class AtencionRepository : GenericRepository<Atencion>, IAtencionRepository
{
    public AtencionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Atencion?> ObtenerDetalleCompletoAsync(Guid id)
    {
        return await Context.Atenciones
            .Include(x => x.Paciente)
            .Include(x => x.Doctor).ThenInclude(d => d.Usuario)
            .Include(x => x.ServicioClinico)
            .Include(x => x.Cita)
            .Include(x => x.Pagos)
            // Carga paralela de submódulos de la historia obstétrica
            .Include(x => x.Anamnesis)
            .Include(x => x.ExamenesFisicos)
            .Include(x => x.TactosVaginales)
            .Include(x => x.Ecografias)
            .Include(x => x.ImpresionDiagnostica)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}

=== "CitaRepository"

csharp
public class CitaRepository : GenericRepository<Cita>, ICitaRepository
{
    public CitaRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExisteInterferenciaHorarioAsync(Guid doctorId, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin, Guid? citaIdExcluir = null)
    {
        var query = Context.Citas.Where(x => x.DoctorId == doctorId && x.Fecha == fecha && x.Estado != EstadoCita.Cancelada && x.Estado != EstadoCita.Eliminada);
        if (citaIdExcluir.HasValue) query = query.Where(x => x.Id != citaIdExcluir.Value);
        
        // Lógica algorítmica: Evaluación de colisión de intervalos horarios de consultorios
        return await query.AnyAsync(x => horaInicio < x.HoraFin && horaFin > x.HoraInicio);
    }
}
=== "UsuarioRepository"

csharp
public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Usuario?> ObtenerPorUserNameAsync(string userName)
    {
        return await Context.Usuarios
            .Include(x => x.UsuarioRoles).ThenInclude(x => x.Rol).ThenInclude(x => x.RolPermisos).ThenInclude(x => x.Permiso)
            .FirstOrDefaultAsync(x => x.UserName == userName);
    }
}
```

### 📄 Servicios de Generación de PDF (Adaptadores de Salida)
El sistema automatiza la generación de entregables legales y médicos estructurando binarios en memoria mediante el motor de diseño fluido QuestPDF. Todos los layouts leen los recursos de marca (Logos e Identidad Corporativa Oro y Azul) del directorio centralizado RECURSOS/.

Flujo del Adaptador de Documentos:
[DTO de Aplicación] ➔ [Maquetación Líquida QuestPDF (A4)] ➔ [Inyección de Estilos de la Clínica] ➔ [Stream de Arreglo de Bytes]
Caso de Uso Implementado: HistoriaClinicaPdfService

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Clinica.Domain.DTOs.Documentos;

namespace Clinica.Infrastructure.Documents;

public class HistoriaClinicaPdfService : IHistoriaClinicaPdfService
{
    private readonly string ColorPrincipal = "#4DB6D2"; // Azul Clínico
    private readonly string ColorSecundario = "#F089A8"; // Oro/Ámbar de Realce

    public byte[] GeneratePdf(HistoriaClinicaPdfDto dto)
    {
        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RECURSOS", "LOGO.jpeg");
        byte[] logoBytes = File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                // Construcción modular del documento por secciones
                page.Header().Element(c => ConstruirEncabezado(c, logoBytes));
                page.Content().Element(c => ConstruirContenido(c, dto));
                page.Footer().Element(ConstruirPiePagina);
            });
        }).GeneratePdf();
    }
    // ... métodos privados de renderizado de tablas, antecedentes obstétricos y partogramas
}
```
### 🌱 Data Seeder Engine (Semillas Iniciales Maestro)
Para garantizar un entorno idéntico entre desarrollo, testing y producción en la nube, el componente DataSeeder orquesta la ejecución transaccional del sembrado inicial de datos maestros mínimos:

Permisos de Sistema: Inserta de forma atómica el catálogo de tokens lógicos de protección de rutas (ej: ATENCION_REGISTRAR, FINANZAS_AUDITAR).

Roles Jerárquicos: Inicializa la matriz corporativa de roles (Administrador, Recepcionista, Doctor, Caja).

Usuarios Base: Registra las credenciales del personal de control aplicando criptografía asimétrica BCrypt sobre los passwords, configurando la bandera de seguridad DebeCambiarContrasena = true. El usuario administrador recibe de manera automática el espectro completo de permisos.

Catálogo del Portafolio de Servicios: Registra los costos base peruanos en Soles (S/.) y duraciones en minutos de los servicios de la clínica (Consultas, Ecografías, Partos, Cesáreas).

Absorción de Pacientes de Ejemplo: Inyecta las pacientes de prueba parametrizando de manera estricta el bloque extendido de datos de filiación.

Apertura de Expedientes: Inicializa de forma sincronizada las carpetas médicas de las pacientes sembradas en la tabla de historiales clínicos, estampando un registro transparente de apertura en la bitácora analítica.

```csharp
public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Forzar la ejecución de migraciones pendientes en Neon.tech antes del sembrado
        await context.Database.MigrateAsync();

        await SeedPermisosAsync(context);
        await SeedRolesAsync(context);
        await SeedUsuariosAsync(context);
        await SeedServiciosClinicosAsync(context);
        await SeedPacientesAsync(context);

        await context.SaveChangesAsync();
    }
}
```

### 🔄 Flujo de Ejecución e Intercambio Arquitectónico
A continuación se describe de manera secuencial y cronológica cómo la Capa de Infraestructura interopera con el resto de los componentes del ecosistema durante un flujo de lectura de historial clínico detallado:

```text
[Cliente Blazor WASM] ➔ Solicita Datos JSON por HTTP GET
                              │
                              ▼
[Controlador HTTP API] ➔ Captura petición y delega a la aplicación
                              │
                              ▼
[Capa de Aplicación] ➔ Invoca el Puerto de Salida Abstracto (IAtencionRepository)
                              │
                              ▼  (Inversión de Control por Inyección de Dependencias)
[Adaptador Infraestructura] ➔ AtencionRepository construye árbol de expresiones LINQ
                              │
                              ▼  (Traducción de Expresiones a Query Relacional)
[EF Core 9 Provider] ➔ Traduce a Sentencia SQL con operadores LEFT JOIN optimizados
                              │
                              ▼  (Ejecución a través de Npgsql Driver)
[PostgreSQL Cloud Neon] ➔ Procesa en el clúster serverless y retorna filas planas
                              │
                              ▼  (Materialización y Grafo de Objetos)
[Capa de Infraestructura] ➔ Convierte registros planos en Entidades de Dominio puras
                              │
                              ▼  (Retorno hacia las Capas Internas)
[Servicio de Aplicación] ➔ Mapea las Entidades a DTOs de respuesta estandarizados
```