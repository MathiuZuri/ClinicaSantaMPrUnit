# ⚙️ Capa de Infraestructura (Persistencia y Adaptadores)

La **Capa de Infraestructura** constituye el **adaptador de salida** de la Arquitectura Hexagonal. Su responsabilidad es implementar los **puertos de salida** (repositorios) definidos en el Dominio, proporcionando mecanismos concretos de persistencia, generación de documentos y comunicación con servicios externos. Esta capa es dependiente de frameworks externos (Entity Framework Core, QuestPDF, etc.), pero su impacto en el núcleo del negocio está completamente aislado gracias a la inversión de dependencias.

!!! info "Frontera Arquitectónica"
    - **Dependencias PERMITIDAS:** Entity Framework Core 9, Npgsql (PostgreSQL), QuestPDF (generación de PDF), BCrypt.Net (hashing de contraseñas), y las librerías estándar de .NET.
    - **Dependencias PROHIBIDAS:** Referencias directas a la capa de API o de Aplicación. Solo puede depender de la capa de Dominio (para conocer las entidades e interfaces) y de paquetes externos de infraestructura.
    - **Comunicación con el Dominio:** Se realiza exclusivamente a través de la implementación de las interfaces de repositorio y servicios definidas en `Clinica.Domain.Interfaces`.

---

## 🗄️ Contexto de Base de Datos (ApplicationDbContext)

El `ApplicationDbContext` es el punto central de acceso a la base de datos PostgreSQL. Hereda de `DbContext` de Entity Framework Core y expone cada entidad del dominio como un `DbSet<T>`. Todas las configuraciones de mapeo se aplican mediante el método `OnModelCreating`, que escanea el ensamblado en busca de clases que implementen `IEntityTypeConfiguration<T>`.

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets para todas las entidades del dominio
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Doctor> Doctores => Set<Doctor>();
    public DbSet<HorarioDoctor> HorariosDoctor => Set<HorarioDoctor>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<ServicioClinico> ServiciosClinicos => Set<ServicioClinico>();
    public DbSet<HistorialClinico> HistorialesClinicos => Set<HistorialClinico>();
    public DbSet<HistorialDetalle> HistorialDetalles => Set<HistorialDetalle>();
    public DbSet<Atencion> Atenciones => Set<Atencion>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<AjusteFinanciero> AjustesFinancieros => Set<AjusteFinanciero>();
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<ComprobanteDetalle> ComprobanteDetalles => Set<ComprobanteDetalle>();
    
    // Módulos obstétricos
    public DbSet<Anamnesis> Anamnesis => Set<Anamnesis>();
    public DbSet<ExamenFisico> ExamenesFisicos => Set<ExamenFisico>();
    public DbSet<TactoVaginal> TactosVaginales => Set<TactoVaginal>();
    public DbSet<EcografiaObstetrica> EcografiasObstetricas => Set<EcografiaObstetrica>();
    public DbSet<ImpresionDiagnostica> ImpresionesDiagnosticas => Set<ImpresionDiagnostica>();
    
    // WhatsApp (exclusivo de Evolution API)
    public DbSet<NotificacionCita> NotificacionesCitas => Set<NotificacionCita>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<MensajeChat> MensajesChat => Set<MensajeChat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}