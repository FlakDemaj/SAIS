using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Application.Common.Interfaces.Services;

using Domain.Common.Enums;

using Infrastructure.Persistence.Context;

using Integration.Tests.Common.Helpers;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Xunit;

namespace Integration.Tests.Common;

[Collection(nameof(IntegrationTestCollection))]
public abstract class TestBase : IAsyncLifetime
{
    protected readonly HttpClient _client;
    protected readonly UserTestRepository _userRepo;
    protected readonly InstituteTestRepository _instituteRepo;

    private readonly SlaisDbContext _dbContext;
    private readonly IServiceScope _scope;

    private static readonly HashSet<string> _excludedSchemas = ["evolve"];

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected TestBase(IntegrationContainerFixture fixture)
    {
        _dbContext = fixture.SlaisDbContext;

        _client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

        _scope = fixture.Factory.Services.CreateScope();
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _userRepo = new UserTestRepository(_dbContext, passwordHasher);
        _instituteRepo = new InstituteTestRepository(_dbContext);
    }

    public virtual async Task InitializeAsync()
    {
        _client.DefaultRequestHeaders.Clear();
        _dbContext.ChangeTracker.Clear();
        await CleanDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        _scope.Dispose();
        _client.Dispose();
        return Task.CompletedTask;
    }

    protected void AuthenticateAs(Roles role, Guid userGuid, Guid instituteGuid)
    {
        var token = GenerateTestToken(role, userGuid, instituteGuid);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected static StringContent BuildJsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    private static string GenerateTestToken(Roles role, Guid userGuid, Guid instituteGuid)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userGuid.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new(ClaimTypes.Role, role.ToString()),
            new("InstituteGuid", instituteGuid.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(IntegrationTestWebApplicationFactory.TestJwtSecret));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: IntegrationTestWebApplicationFactory.TestIssuer,
            audience: IntegrationTestWebApplicationFactory.TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task CleanDatabaseAsync()
    {
        var qualifiedTableNames = _dbContext.Model
            .GetEntityTypes()
            .Select(e => new
            {
                Schema = e.GetSchema() ?? "public",
                Table = e.GetTableName()
            })
            .Where(e => e.Table is not null)
            .Where(e => !_excludedSchemas.Contains(e.Schema))
            .Select(e => $"\"{SanitizeIdentifier(e.Schema)}\".\"{SanitizeIdentifier(e.Table!)}\"")
            .Distinct();

        var tables = string.Join(", ", qualifiedTableNames);

#pragma warning disable EF1002
        await _dbContext.Database.ExecuteSqlRawAsync(
            $"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE");
#pragma warning restore EF1002
    }

    private static string SanitizeIdentifier(string identifier)
    {
        return identifier.Replace("\"", string.Empty);
    }
}
