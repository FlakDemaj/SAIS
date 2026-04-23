using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Application.Authentication.Commands.Login;
using Application.Common.Interfaces.Services;

using Domain.Common.Enums;

using Infrastructure.Configurations;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Infrastructure.InternalServices;

public class TokenService : ITokenService
{
    private readonly AccessTokenOptions _accessTokenOptions;

    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(
        IOptions<AccessTokenOptions> tokenOptions)
    {
        _accessTokenOptions = tokenOptions.Value;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public GeneratedAccessTokenResult GenerateAccessToken(
        Guid userGuid,
        Roles userRole,
        Guid instituteGuid)
    {
        var claims = CreateUserClaims(userGuid, userRole, instituteGuid);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_accessTokenOptions.Key));

        var creds = new SigningCredentials(key,
            SecurityAlgorithms.HmacSha512);

        var token = CreateToken(
            claims, creds);

        var accessToken = _tokenHandler.WriteToken(token);

        return new GeneratedAccessTokenResult
        {
            AccessToken = accessToken,
            AccessTokenExpiresInMinutes = _accessTokenOptions.ExpiresInMinutes
        };
    }

    private JwtSecurityToken CreateToken(
        List<Claim> claims,
        SigningCredentials creds)
    {
        var token = new JwtSecurityToken
        (
            issuer: _accessTokenOptions.Issuer,
            audience: _accessTokenOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenOptions.ExpiresInMinutes),
            signingCredentials: creds
        );

        return token;
    }

    private static List<Claim> CreateUserClaims(
        Guid userGuid,
        Roles userRole,
        Guid instituteGuid)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userGuid.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),

            new(ClaimTypes.Role, userRole.ToString()),
            new("InstituteGuid", instituteGuid.ToString()),
        };

        return claims;
    }
}
