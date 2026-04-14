using Domain.Common.Enums;

using FluentAssertions;

using Infrastructure.Configurations;
using Infrastructure.InternalServices;

using Microsoft.Extensions.Options;

using SLAIS.Domain.Users;

using Xunit;

namespace Infrastructure.Tests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    private readonly IOptions<AccessTokenOptions> _accessTokenOptions;

    public TokenServiceTests()
    {
        _accessTokenOptions = Options.Create(new AccessTokenOptions
        {
            Audience = "TestAudience",
            ExpiresInMinutes = 30,
            Issuer = "TestIssuer",
            Key = "SLAIS-Testing-Secret-Key-For-HmacSha512-Must-Be-64-Characters-!!"
        });

        _tokenService = new TokenService(_accessTokenOptions);
    }

    [Fact]
    public void GenerateAccessToken_CreateToken()
    {
        var token = _tokenService.GenerateAccessToken(
            Guid.CreateVersion7(),
            Roles.Teacher,
            Guid.CreateVersion7());

        token.Should().NotBeNull();

        token.AccessToken.Should().NotBeEmpty();
        token.AccessTokenExpiresInMinutes.Should().Be(_accessTokenOptions.Value.ExpiresInMinutes);
    }
}
