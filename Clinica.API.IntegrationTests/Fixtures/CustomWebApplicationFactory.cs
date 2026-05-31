using Clinica.Infrastructure.Data;
using Clinica.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Clinica.API.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var testingConfiguration = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,

                ["Jwt:Key"] = "CLINICA_SANTA_MONICA_TEST_SECRET_KEY_123456789_ABC",
                ["Jwt:Issuer"] = "ClinicaSantaMonica.Testing",
                ["Jwt:Audience"] = "ClinicaSantaMonica.Testing",
                ["Jwt:ExpireMinutes"] = "120",

                // Para evitar errores si algún servicio de WhatsApp se resuelve en testing.
                ["WhatsApp:Enabled"] = "false",
                ["WhatsApp:BaseUrl"] = "https://localhost",
                ["WhatsApp:InstanceName"] = "testing",
                ["WhatsApp:ApiKey"] = "testing-key"
            };

            config.AddInMemoryCollection(testingConfiguration);
        });

        builder.ConfigureServices(services =>
        {
            // Quitamos background services para que Evolution/recordatorios
            // no se ejecuten durante pruebas de integración.
            services.RemoveAll<IHostedService>();

            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureDeleted();
            db.Database.Migrate();

            DataSeeder.SeedAsync(db).GetAwaiter().GetResult();
        });
    }
}