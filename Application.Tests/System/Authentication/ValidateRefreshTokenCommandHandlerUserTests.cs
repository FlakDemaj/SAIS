using Application.Authentication;
using Application.Authentication.Commands.Login;
using Application.Authentication.Commands.ValidateRefreshToken;
using Application.Common.Interfaces.Services;
using Application.Interfaces;
using Application.System.Authentication.Commands.ValidateRefreshToken;
using Application.Utils.Interfaces.Transaction;
using Application.Utils.Logger;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

using FluentAssertions;

using NSubstitute;

using SLAIS.Domain.Users;

using Tests.Shared.TestDataCreator;

using Xunit;

namespace Application.Tests.System.Authentication;

public class ValidateRefreshTokenCommandHandlerTests
{
    private readonly IUserRepository _userRepository;

    private readonly ITokenService _tokenService;

    private readonly ValidateRefreshTokenCommandHandler _handler;

    public ValidateRefreshTokenCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenService = Substitute.For<ITokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ISlaisLogger<ValidateRefreshTokenCommandHandler>>();
        var mapper = Substitute.For<IMapper>();

        _handler = new ValidateRefreshTokenCommandHandler(
            logger,
            mapper,
            _userRepository,
            unitOfWork,
            _tokenService);
    }

    #region HandleAsync - Success

    [Fact]
    public async Task HandleAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var command = BuildValidCommand();
        var user = UserTestData.CreateUser();

        UserTestData.CreateRefreshToken(user, refreshTokenGuid: command.RefreshToken);

        _userRepository
            .GetUserWithRefreshTokensByGuidAsync(command.RefreshToken)
            .Returns(user);

        var expectedResult = new GeneratedAccessTokenResult
        {
            AccessToken = "access-token",
            AccessTokenExpiresInMinutes = 900
        };

        _tokenService
            .GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<Roles>(), Arg.Any<Guid>())
            .Returns(expectedResult);

        var result = await _handler.HandleAsync(command, null, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be(expectedResult.AccessToken);
        result.AccessTokenExpiresInMinutes.Should().Be(expectedResult.AccessTokenExpiresInMinutes);
    }

    #endregion

    #region HandleAsync - Failed

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenUserNotFound()
    {
        var command = BuildValidCommand();

        _userRepository
            .GetUserWithRefreshTokensByGuidAsync(command.RefreshToken)
            .Returns((UserEntity?)null);

        var act = async () =>
        {
            return await _handler.HandleAsync(command, null, CancellationToken.None);
        };

        await act
            .Should()
            .ThrowAsync<SlaisException>()
            .Where(x => x.ErrorCode == (int)AuthErrorCodes.NoUserWithThisToken);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenTokenIsRevoked()
    {
        var command = BuildValidCommand();
        var user = UserTestData.CreateUser();

        UserTestData.CreateRevokedRefreshToken(user);

        _userRepository
            .GetUserWithRefreshTokensByGuidAsync(command.RefreshToken)
            .Returns(user);

        var act = async () =>
        {
            return await _handler.HandleAsync(command, null, CancellationToken.None);
        };

        await act
            .Should()
            .ThrowAsync<SlaisException>()
            .Where(x => x.ErrorCode == (int)AuthErrorCodes.NoValidTokenFound);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenTokenIsExpired()
    {
        var command = BuildValidCommand();
        var user = UserTestData.CreateUser();

        UserTestData.CreateExpiredRefreshToken(user);

        _userRepository
            .GetUserWithRefreshTokensByGuidAsync(command.RefreshToken)
            .Returns(user);

        var act = async () =>
        {
            return await _handler.HandleAsync(command, null, CancellationToken.None);
        };

        await act
            .Should()
            .ThrowAsync<SlaisException>()
            .Where(x => x.ErrorCode == (int)AuthErrorCodes.NoValidTokenFound);
    }

    #endregion

    #region Helpers

    private static ValidateRefreshTokenCommand BuildValidCommand()
    {
        return new ValidateRefreshTokenCommand
        {
            RefreshToken = Guid.CreateVersion7()
        };
    }

    #endregion
}
