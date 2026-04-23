using EvolveDb;

using Infrastructure.Configurations;
using Infrastructure.Persistence.Context;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Testcontainers.PostgreSql;

using Xunit;

namespace Integration.Tests.Common;

public class IntegrationContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;

    public SlaisDbContext SlaisDbContext { get; private set; } = null!;
    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;
    public string ConnectionString { get; private set; } = null!;

    public IntegrationContainerFixture()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        ConnectionString = _postgresContainer.GetConnectionString();

        var databaseOptions = Options.Create(new DatabaseOptions
        {
            ConnectionString = ConnectionString
        });

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

        SlaisDbContext = new SlaisDbContext(databaseOptions, loggerFactory);

        RunEvolveMigrations(ConnectionString);

        Factory = new IntegrationTestWebApplicationFactory(ConnectionString);
    }

    private static void RunEvolveMigrations(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            CommandTimeout = 3600
        };

        using var connection = new NpgsqlConnection(builder.ConnectionString);

        var evolve = new Evolve(connection)
        {
            Locations = [CreateMigrationsFolderPath()],
            Schemas = ["evolve"],
            IsEraseDisabled = false,
            OutOfOrder = true
        };

        evolve.Migrate();
    }

    private static string CreateMigrationsFolderPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Persistence", "Migrations");
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await SlaisDbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}
