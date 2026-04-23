using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Presentation.Server;

namespace Integration.Tests.Common;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public const string TestJwtSecret =
        "3fLoEsirN3qEnizXuF1ONUzsnbLcRGmojTiT7oqrsuYg2twptjhJYVyCq5rDM95uz4CRJAZZ513MDUgSiFhIT2";
    public const string TestIssuer = "TestIssuer";
    public const string TestAudience = "TestAudience";

    public IntegrationTestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Connection_String"] = _connectionString
        }));

        builder.ConfigureLogging(logging => logging.ClearProviders());
    }
}
