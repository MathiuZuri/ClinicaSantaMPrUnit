using System.Net.Http.Headers;
using Clinica.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.API.IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly List<IServiceScope> _scopes = [];

    protected readonly PostgreSqlFixture PostgreSqlFixture;

    protected CustomWebApplicationFactory Factory = null!;
    protected HttpClient Client = null!;

    protected IntegrationTestBase(PostgreSqlFixture postgreSqlFixture)
    {
        PostgreSqlFixture = postgreSqlFixture;
    }

    protected ApplicationDbContext CreateDbContext()
    {
        var scope = Factory.Services.CreateScope();
        _scopes.Add(scope);

        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected IServiceScope CreateScope()
    {
        var scope = Factory.Services.CreateScope();
        _scopes.Add(scope);

        return scope;
    }

    protected void SetBearerToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthorization()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task LoginAsAdminAsync()
    {
        var token = await Helpers.AuthTestHelper.LoginAsAdminAndGetTokenAsync(Client);
        SetBearerToken(token);
    }

    public virtual Task InitializeAsync()
    {
        Factory = new CustomWebApplicationFactory(PostgreSqlFixture.ConnectionString);
        Client = Factory.CreateClient();

        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        foreach (var scope in _scopes)
        {
            scope.Dispose();
        }

        Client.Dispose();
        Factory.Dispose();

        return Task.CompletedTask;
    }
}