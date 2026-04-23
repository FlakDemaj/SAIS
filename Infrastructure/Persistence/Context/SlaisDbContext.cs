using Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Z.EntityFramework.Plus;

namespace Infrastructure.Persistence.Context;

public class SlaisDbContext : DbContext
{
    private readonly string _connectionString;

    private ILoggerFactory LoggerFactory { get; init; }

    public SlaisDbContext(
        IOptions<DatabaseOptions> databaseOptions,
        ILoggerFactory iLoggerFactory)
    {
        _connectionString = databaseOptions.Value.ConnectionString;
        LoggerFactory = iLoggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        optionsBuilder.UseNpgsql(
            _connectionString,
            o => o.CommandTimeout(120));

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
        {
            optionsBuilder.UseLoggerFactory(LoggerFactory);
        }

        EntityFrameworkPlusManager.IsCommunity = true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assembly = typeof(SlaisDbContext).Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}
