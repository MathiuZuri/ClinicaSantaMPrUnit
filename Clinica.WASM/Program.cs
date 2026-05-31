using Clinica.WASM;
using Clinica.WASM.Services.Api;
using Clinica.WASM.Services.Auth;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddScoped<ApiErrorService>();
builder.Services.AddScoped<AuthRedirectService>();
//funcionalidades
// Servicios con HttpClient configurado y AuthHeaderHandler
builder.Services.AddHttpClient<PacienteApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<CitaApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<ServicioClinicoApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

// AuthApiService también (si aún no lo arreglaste)
builder.Services.AddHttpClient<AuthApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<DoctorApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<HorarioDoctorApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<HistorialClinicoApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<UsuarioApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<RolApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<PermisoApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<PagoApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<FinanzasApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
}).AddHttpMessageHandler<AuthHeaderHandler>();


builder.Services.AddHttpClient("ClinicaApi", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7241/"); // NOSONAR
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

await builder.Build().RunAsync();