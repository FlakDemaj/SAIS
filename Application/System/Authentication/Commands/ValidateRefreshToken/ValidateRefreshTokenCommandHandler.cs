using Application.Authentication;
using Application.Authentication.Commands.Login;
using Application.Authentication.Commands.ValidateRefreshToken;
using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.Interfaces.Repositorys;
using Application.Common.Interfaces.Services;
using Application.Interfaces;
using Application.Utils.Interfaces.Transaction;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Exceptions;

namespace Application.System.Authentication.Commands.ValidateRefreshToken;

public class ValidateRefreshTokenCommandHandler
    : BaseHandler<ValidateRefreshTokenCommandHandler>,
        IRequestHandler<ValidateRefreshTokenCommand, AccessTokenResponseDto>
{
    private readonly IUserRepository _userRepository;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ITokenService _tokenService;

    public ValidateRefreshTokenCommandHandler(
        ISlaisLogger<ValidateRefreshTokenCommandHandler> logger,
        IMapper mapper,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
        : base(mapper, logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<AccessTokenResponseDto> HandleAsync(
        ValidateRefreshTokenCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserWithRefreshTokensByGuidAsync(request.RefreshToken);

        if (user == null)
        {
            _logger.LogError($"Refresh Token: {request.RefreshToken} has no user.", null);
            throw new SlaisException(AuthErrorCodes.NoUserWithThisToken);
        }

        var isTokenValid = user.ValidateRefreshToken(request.RefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!isTokenValid)
        {
            throw new SlaisException(AuthErrorCodes.NoValidTokenFound);
        }

        var accessToken = _tokenService.GenerateAccessToken(
            user.Guid,
            user.Role,
            user.InstituteGuid);

        return new AccessTokenResponseDto
        {
            AccessToken = accessToken.AccessToken,
            AccessTokenExpiresInMinutes = accessToken.AccessTokenExpiresInMinutes
        };
    }
}
