using Xunit;

namespace Clinica.API.IntegrationTests.Fixtures;

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<PostgreSqlFixture>
{
}