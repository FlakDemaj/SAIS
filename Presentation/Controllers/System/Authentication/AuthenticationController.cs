using System.Net;

using Application.Authentication;
using Application.Authentication.Commands;
using Application.Authentication.Commands.Login;
using Application.Authentication.Commands.ValidateRefreshToken;
using Application.Utils.Interfaces.Mediator;

using Domain.Common.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.System.Authentication;

public class AuthenticationController : BaseRestController
{
    public AuthenticationController(
        IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AccessTokenResponseDto>> LoginAsync(
        [FromBody] LoginRequest loginRequest,
        CancellationToken cancellationToken)
    {
        var loginCommand = MapLoginRequest(loginRequest, HttpContext);
        var tokens = await _mediator.SendAsync(loginCommand, null, cancellationToken);

        HttpContext.Response.Cookies.Append(
            "RefreshToken",
            tokens.RefreshToken.RefreshToken.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(tokens.RefreshToken.RefreshTokenExpiresInDays)
            });

        return Ok(
            new AccessTokenResponseDto
            {
                AccessToken = tokens.GeneratedAccessToken.AccessToken,
                AccessTokenExpiresInMinutes = tokens.GeneratedAccessToken.AccessTokenExpiresInMinutes
            }
        );
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<AccessTokenResponseDto>> ValidateRefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        var refreshTokenCookie = HttpContext.Request.Cookies["RefreshToken"];

        if (string.IsNullOrWhiteSpace(refreshTokenCookie))
        {
            throw new SlaisException(AuthErrorCodes.NoValidTokenFound);
        }

        if (!Guid.TryParse(refreshTokenCookie, out var refreshTokenGuid))
        {
            throw new SlaisException(AuthErrorCodes.NoValidTokenFound);
        }

        var refreshTokenCommand = new ValidateRefreshTokenCommand
        {
            RefreshToken = refreshTokenGuid
        };

        return await _mediator.SendAsync(refreshTokenCommand, null, cancellationToken);
    }

    private static LoginCommand MapLoginRequest(
        LoginRequest loginRequest,
        HttpContext context)
    {
        return new LoginCommand
        {
            LoginName = loginRequest.LoginName,
            Password = loginRequest.Password,
            IpAddress = context.Connection.RemoteIpAddress ?? IPAddress.Loopback,
            DeviceName = loginRequest.DeviceName,
            DeviceGuid = loginRequest.DeviceGuid
        };
    }
}
