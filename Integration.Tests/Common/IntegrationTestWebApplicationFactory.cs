using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Presentation.Server;

namespace Integration.Tests.Common;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public const string TestJwtSecret = "this-is-a-super-secret-test-key-12345678";
    public const string TestIssuer = "TestIssuer";
    public const string TestAudience = "TestAudience";

    public IntegrationTestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Connection_String"] = _connectionString,
                ["AccessToken:Issuer"] = TestIssuer,
                ["AccessToken:Audience"] = TestAudience,
                ["AccessToken:Key"] = TestJwtSecret,
                ["AccessToken:ExpiresInMinutes"] = "15",
                ["RefreshToken:ExpiresInDays"] = "30",
                ["CommonOptions:MaxLoginAttempts"] = "5",
                ["Sentry:Dsn"] = ""
            });
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = TestIssuer,
                    ValidAudience = TestAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(TestJwtSecret))
                };
            });
        });

        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.UseEnvironment("Testing");
    }
}
