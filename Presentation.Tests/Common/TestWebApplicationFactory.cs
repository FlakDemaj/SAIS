using System.Text;

using Application.Utils.Interfaces.Mediator;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using NSubstitute;

using SLAIS.Presentation.Server;

namespace Presentation.Tests.Common;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly IMediator MediatorMock = Substitute.For<IMediator>();

    public const string TestJwtSecret = "this-is-a-super-secret-test-key-12345678";
    public const string TestIssuer = "TestIssuer";
    public const string TestAudience = "TestAudience";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var mediatorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMediator));

            if (mediatorDescriptor != null)
            {
                services.Remove(mediatorDescriptor);
            }

            services.AddSingleton(MediatorMock);

            var jwtDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(JwtBearerOptions));

            if (jwtDescriptor != null)
            {
                services.Remove(jwtDescriptor);
            }

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

        builder.UseEnvironment("Testing");
    }
}
