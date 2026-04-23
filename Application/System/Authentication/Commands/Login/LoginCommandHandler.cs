using Application.Authentication;
using Application.Authentication.Commands;
using Application.Authentication.Commands.Login;
using Application.Common;
using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.Interfaces.Repositorys;
using Application.Common.Interfaces.Services;
using Application.Common.Options;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Exceptions;

using Microsoft.Extensions.Options;

using SLAIS.Domain.Users;

namespace Application.System.Authentication.Commands.Login;

public class LoginCommandHandler :
    BaseHandler<LoginCommandHandler>,
    IRequestHandler<LoginCommand, GeneratedTokenResult>
{
    private readonly IUserRepository _userRepository;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly CommonOptions _commonOptions;


    public LoginCommandHandler(
        IUserRepository userRepository,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        ITokenService tokenService,
        ISlaisLogger<LoginCommandHandler> logger,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IOptions<CommonOptions> commonOptions)
        : base(mapper, logger)
    {
        _userRepository = userRepository;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _commonOptions = commonOptions.Value;
    }

    public async Task<GeneratedTokenResult> HandleAsync(
        LoginCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        var user = await CheckUser(request.LoginName);

        // Check if the user is already logged in with the same device.
        // If so, revoke the old refresh token and create a new one.
        user.RevokeRefreshTokens(request.DeviceGuid);

        CheckPassword(
            user,
            request.Password);

        return GenerateToken(
                user,
                request);
    }

    private async Task<UserEntity> CheckUser(
        string loginName)
    {
        var user = await _userRepository
            .GetUserByUsernameOrEmailWithRefreshTokenAsync(loginName);

        if (user == null)
        {
            throw new SlaisException(AuthErrorCodes.NoUserWithThisName);
        }

        user.CheckUser();

        return user;
    }

    private void CheckPassword(
        UserEntity user,
        string password)
    {
        if (user.HashedPassword == null)
        {
            throw new SlaisException(CommonErrorCodes.DefaultErrorCode);
        }

        var checkPassword = _passwordHasher.Verify(
            password,
            user.HashedPassword);

        if (checkPassword)
        {
            user.SetLoginAttemptsToZero();
            return;
        }

        user.IncrementWrongLoginAttempts(_commonOptions.MaxLoginAttempts);

        throw new SlaisException(AuthErrorCodes.WrongPassword);
    }

    private GeneratedTokenResult GenerateToken(
        UserEntity user,
        LoginCommand request)
    {
        var accessToken = _tokenService.GenerateAccessToken(
            user.Guid,
            user.Role,
            user.InstituteGuid);

        var refreshToken = user.CreateRefreshToken(
            _refreshTokenOptions.ExpiresInDays,
            deviceName: request.DeviceName,
            ipAddress: request.IpAddress,
            deviceGuid: request.DeviceGuid);

        return new GeneratedTokenResult
        {
            GeneratedAccessToken = accessToken,
            RefreshToken = new GeneratedRefreshTokenResult
            {
                RefreshToken = refreshToken.RefreshToken,
                RefreshTokenExpiresInDays = _refreshTokenOptions.ExpiresInDays
            }
        };
    }
}
