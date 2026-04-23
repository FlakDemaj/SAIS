using System.Security.Claims;
using System.Text;

using Application;
using Application.Common.Options;

using Infrastructure;
using Infrastructure.Configurations;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Presentation.Middlewares;

namespace Presentation.Server;

public static class Startup
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        EnableCors(builder.Services);

        ConfigureSentry(builder);
        ConfigureLogger(builder);
        ConfigureLayers(builder);
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        ConfigureMiddleware(app);
    }

    private static void ConfigureLayers(WebApplicationBuilder builder)
    {
        AddApplicationLayer(builder);
        AddPresentationLayer(builder.Services, builder.Configuration);
        AddInfrastructureLayer(builder);
    }

    private static void AddPresentationLayer(
        IServiceCollection services,
         ConfigurationManager configuration)
    {
        services.AddControllers();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document =>
            {
                var securityRequirement = new OpenApiSecurityRequirement();

                var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);

                securityRequirement.Add(schemeReference, []);

                return securityRequirement;
            });
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RoleClaimType = ClaimTypes.Role,
                ValidIssuer = configuration["AccessToken:Issuer"],
                ValidAudience = configuration["AccessToken:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["AccessToken:Key"]!))
            });
    }

    private static void AddInfrastructureLayer(WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure();

        ConfigureOptions(builder);
    }

    private static void AddApplicationLayer(WebApplicationBuilder builder)
    {
        builder.Services.AddApplicationLayer();
    }

    private static void ConfigureSentry(WebApplicationBuilder builder)
    {
        builder.WebHost.UseSentry(options =>
        {
            options.Dsn = !builder.Environment.IsProduction() ? "" : builder.Configuration["Sentry:Dsn"];
            options.Debug = false;
            options.TracesSampleRate = 0.10;
            options.MinimumEventLevel = LogLevel.Warning;
            options.AttachStacktrace = true;
        });
    }

    private static void ConfigureLogger(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = false;
            options.TimestampFormat = "HH:mm:ss ";
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.UseUtcTimestamp = true;
            options.ColorBehavior = builder.Environment.IsProduction()
                ? LoggerColorBehavior.Disabled
                : LoggerColorBehavior.Enabled;
        });

        builder.Logging.SetMinimumLevel(LogLevel.Trace);
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseSentryTracing();
        app.UseCors("AllowOrigin");
        app.UseAuthentication();
        app.UseAuthorization();
        EnableMiddlewares(app);
        app.MapControllers();
    }

    private static void EnableCors(IServiceCollection service)
    {
        service.AddCors(option => option.AddPolicy("AllowOrigin",
                policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()));
    }

    private static void EnableMiddlewares(IApplicationBuilder builder)
    {
        builder.UseMiddleware<ExceptionMiddleware>();
        builder.UseMiddleware<AuthenticationMiddleware>();
    }

    private static void ConfigureOptions(WebApplicationBuilder builder)
    {
        builder.Services.Configure<DatabaseOptions>(
            builder.Configuration.GetSection("Database"));
        builder.Services.Configure<AccessTokenOptions>(
            builder.Configuration.GetSection("AccessToken"));
        builder.Services.Configure<RefreshTokenOptions>(
            builder.Configuration.GetSection("RefreshToken"));
        builder.Services.Configure<CommonOptions>(
            builder.Configuration.GetSection("CommonOptions"));

    }
}
