using System.Text;
using Clinica.API.Authorization;
using Clinica.API.Configurations;
using Clinica.API.Filters;
using Clinica.API.Helpers;
using Clinica.API.Hubs;
using Clinica.API.Middlewares;
using Clinica.API.Services;
using Clinica.API.Services.Imp;
// --- NUEVOS USINGS DE MÓDULOS OBSTÉTRICOS ---
using Clinica.API.Services.Imp.ATENCIONES;
using Clinica.Domain.Interfaces;
using Clinica.Domain.Interfaces.ATENCIONES;
// --------------------------------------------
using Clinica.Infrastructure.Data;
using Clinica.Infrastructure.Data.Seeds;
using Clinica.Infrastructure.Documents.Comprobantes.Services;
using Clinica.Infrastructure.Repositories;
// --- NUEVOS USINGS DE REPOSITORIOS OBSTÉTRICOS ---
using Clinica.Infrastructure.Repositories.ATENCIONES;
// -------------------------------------------------
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Clinica.API.Services.Background;
using Clinica.API.Services.Imp.WhastAppImp;
using Clinica.API.Services.Imp.WhatsApp;
using Clinica.Domain.PDFsDto.Interfacespdf;
using Clinica.Infrastructure.Documents.Comprobantes.Pdfservicios;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// CONFIGURACIÓN GENERAL - Para Program.cs
// ==========================================================
builder.Services.AddScoped<AuditoriaAutomaticaFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditoriaAutomaticaFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(
    ValidationResponseConfig.ConfigurarRespuestasDeValidacion
);

builder.Services.AddEndpointsApiExplorer();

// Funcionalidad Para poder leer las xml de los controlers los caules serviran para la documentacion
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Clínica Santa Mónica API",
        Version = "v1",
        Description = "API del sistema de gestión clínica SIGEC."
    });

    // Incluir los comentarios XML de tus controladores
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IUsuarioActualService, UsuarioActualService>();

// ==========================================================
// AUTENTICACIÓN JWT
// ==========================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

// ==========================================================
// AUTORIZACIÓN POR PERMISOS
// ==========================================================
builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in PermisosPolicies.Todos)
    {
        options.AddPolicy(permiso, policy =>
            policy.RequireClaim("permiso", permiso));
    }
});

// ==========================================================
// BASE DE DATOS
// ==========================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================================
// REPOSITORIO GENÉRICO
// ==========================================================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ==========================================================
// REPOSITORIOS
// ==========================================================
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IHorarioDoctorRepository, HorarioDoctorRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();

builder.Services.AddScoped<IServicioClinicoRepository, ServicioClinicoRepository>();
builder.Services.AddScoped<IHistorialClinicoRepository, HistorialClinicoRepository>();
builder.Services.AddScoped<IHistorialDetalleRepository, HistorialDetalleRepository>();
builder.Services.AddScoped<IAtencionRepository, AtencionRepository>();

// --- NUEVOS REPOSITORIOS OBSTÉTRICOS ---
builder.Services.AddScoped<IAnamnesisRepository, AnamnesisRepository>();
builder.Services.AddScoped<IExamenFisicoRepository, ExamenFisicoRepository>();
builder.Services.AddScoped<ITactoVaginalRepository, TactoVaginalRepository>();
builder.Services.AddScoped<IEcografiaObstetricaRepository, EcografiaObstetricaRepository>();
builder.Services.AddScoped<IImpresionDiagnosticaRepository, ImpresionDiagnosticaRepository>();
// ---------------------------------------

builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IAjusteFinancieroRepository, AjusteFinancieroRepository>();

builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
builder.Services.AddScoped<INotificacionCitaRepository, NotificacionCitaRepository>();

// ==========================================================
// SERVICIOS
// ==========================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IHorarioDoctorService, HorarioDoctorService>();
builder.Services.AddScoped<ICitaService, CitaService>();

builder.Services.AddScoped<IServicioClinicoService, ServicioClinicoService>();
builder.Services.AddScoped<IHistorialClinicoService, HistorialClinicoService>();
builder.Services.AddScoped<IAtencionService, AtencionService>();

// --- NUEVOS SERVICIOS OBSTÉTRICOS ---
builder.Services.AddScoped<IAnamnesisService, AnamnesisService>();
builder.Services.AddScoped<IExamenFisicoService, ExamenFisicoService>();
builder.Services.AddScoped<ITactoVaginalService, TactoVaginalService>();
builder.Services.AddScoped<IEcografiaObstetricaService, EcografiaObstetricaService>();
builder.Services.AddScoped<IImpresionDiagnosticaService, ImpresionDiagnosticaService>();

// Servicios de PDF
builder.Services.AddScoped<IHistoriaClinicaPdfService, HistoriaClinicaPdfService>();
builder.Services.AddScoped<IResumenPartoPdfService, ResumenPartoPdfService>();
builder.Services.AddScoped<IReporteFinancieroPdfService, ReporteFinancieroPdfService>();
builder.Services.AddScoped<ICertificadoTrabajoPdfService, CertificadoTrabajoPdfService>();

// ------------------------------------

builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IFinanzasService, FinanzasService>();

builder.Services.AddScoped<IComprobanteService, ComprobanteService>();
builder.Services.AddScoped<IComprobantePdfService, ComprobantePdfService>();

builder.Services.AddSignalR();
builder.Services.Configure<WhatsAppOptions>(builder.Configuration.GetSection("WhatsApp"));

builder.Services.AddHttpClient<INotificacionWhatsAppService, EvolutionWhatsAppService>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<WhatsAppOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<RecordatorioCitasBackgroundService>();

// ==========================================================
// CORS
// ==========================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirBlazor", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7299",
                "http://localhost:5091",
                "https://salmon-bush-08c1e7510.7.azurestaticapps.net" // Reemplaza con tu URL real de Azure Static Web Apps cuando se cree
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ==========================================================
// QUESTPDF
// ==========================================================
QuestPDF.Settings.License = LicenseType.Community;

// ==========================================================
// PIPELINE HTTP
// ==========================================================
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// ─── MITIGACIÓN DE ALERTAS OWASP ZAP (MIDDLEWARE DE CABECERAS DE SEGURIDAD) ───
app.Use(async (context, next) =>
{
    // Generar un nonce aleatorio (base64 de 16 bytes)
    var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        .Replace("+", "-").Replace("/", "_").Replace("=", "");

    // Guardar el nonce en el contexto para usarlo en las vistas
    context.Items["CspNonce"] = nonce;

    // Cabeceras fijas
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // CSP con nonce y strict-dynamic
    context.Response.Headers.Append("Content-Security-Policy",
        $"default-src 'self'; " +
        $"script-src 'self' 'nonce-{nonce}' 'strict-dynamic'; " +
        $"style-src 'self' 'nonce-{nonce}'; " +
        $"frame-ancestors 'none'; " +
        $"object-src 'none'; " +
        $"base-uri 'self';");

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        //https://localhost:7241/swagger/index.html
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinica API v1");
    });
}
else
{
    // 4. Solución Strict-Transport-Security (HSTS) para producción
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("PermitirBlazor");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

// ==================
// SEEDER
// ==================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.Run(); // NOSONAR Password=root123456

public partial class Program
{
}