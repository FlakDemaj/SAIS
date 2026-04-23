using System.Net;

using Domain.Public.Users;
using Domain.Systems.RefreshToken;
using Domain.Tests.Utils.Extensions;

using FluentAssertions;

using Tests.Domain.Shared.TestDataCreator;

using Xunit;

namespace Domain.Tests.Public.Users;

public class UserRefreshTokenTest
{
    #region Create

    [Fact]
    public void CreateRefreshTokenForUser_NotBeNull()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            30,
            Guid.CreateVersion7(),
            "Iphone Galaxy S8 Ultra",
            IPAddress.Loopback);

        user.RefreshTokens.Should().NotBeEmpty();
        user.RefreshTokens.Should().ContainSingle();
        user.RefreshTokens.First().Should().BeEquivalentTo(token);
    }

    [Fact]
    public void CreateRefreshTokenForUser_ShouldSetCorrectUserGuid()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            7,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        token.UserGuid.Should().Be(user.Guid);
    }

    [Fact]
    public void CreateRefreshTokenForUser_ShouldSetCorrectRefreshTokenGuid()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            7,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        token.RefreshToken.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateRefreshTokenForUser_ShouldSetCorrectDeviceAttributes()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            7,
            Guid.CreateVersion7(),
            "Iphone Galaxy S8 Ultra",
            IPAddress.Loopback);

        token.DeviceName.Should().Be("Iphone Galaxy S8 Ultra");
        token.DeviceGuid.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateRefreshTokenForUser_ShouldSetCorrectUser()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            7,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        token.UserGuid.Should().Be(user.Guid);
    }

    [Fact]
    public void CreateRefreshTokenForUser_ShouldSetCorrectDateTimes()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            30,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        token.CreatedDate.Should().BeAfter(DateTime.MinValue);
        token.ExpirationDate.Should().BeAfter(DateTime.UtcNow);
        token.LastUsedDate.Should().BeAfter(DateTime.MinValue);
        token.RevokedDate.Should().BeNull();
    }

    [Fact]
    public void CreateRefreshTokenForUser_ShouldSetCorrectRevoke()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            7,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        token.Revoked.Should().BeFalse();
    }

    [Fact]
    public void CreateRefreshTokenForUser_ShouldSetCorrectIpAddress()
    {
        var user = UserTestData.CreateUser();
        var token = user.CreateRefreshToken(
            7,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        token.IpAddress.Should().Be(IPAddress.Loopback);
    }

    [Fact]
    public void CreateRefreshToken_ShouldThrowException_WhenExpireDaysIsZero()
    {
        var user = UserTestData.CreateUser();

        var act = () => user.CreateRefreshToken(
                        0,
                        Guid.CreateVersion7(),
                        "Test Device",
                        IPAddress.Loopback);

        act.ThrowsException(RefreshTokenErrorCodes.InvalidExpiration);
    }

    [Fact]
    public void CreateRefreshToken_ShouldThrowException_WhenDeviceGuidIsEmpty()
    {
        var user = UserTestData.CreateUser();

        var act = () => user.CreateRefreshToken(
                        7,
                        Guid.Empty,
                        "Test Device",
                        IPAddress.Loopback);

        act.ThrowsException(RefreshTokenErrorCodes.InvalidDeviceGuid);
    }

    [Fact]
    public void CreateRefreshToken_ShouldThrowException_WhenDeviceNameIsEmpty()
    {
        var user = UserTestData.CreateUser();

        var act = () => user.CreateRefreshToken(
                        7,
                        Guid.CreateVersion7(),
                        " ",
                        IPAddress.Loopback);

        act.ThrowsException(RefreshTokenErrorCodes.InvalidDeviceName);
    }

    #endregion

    #region Get

    [Fact]
    public void GetRefreshTokenExpireDate_ShouldReturnCorrectExpireDate()
    {
        var user = UserTestData.CreateUser();
        var token = UserTestData.CreateRefreshToken(user, expiresInDays: 30);

        token.GetExpirationInDays().Should().Be(30);
    }

    #endregion

    #region Validate

    [Fact]
    public void ValidateRefreshToken_ShouldReturnTrue()
    {
        var user = UserTestData.CreateUser();
        var token = UserTestData.CreateRefreshToken(user);

        var isValid = user.ValidateRefreshToken(token.RefreshToken);

        isValid.Should().BeTrue();
        token.LastUsedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ValidateRefreshToken_ShouldThrowException_WhenUserIsBlocked()
    {
        var user = UserTestData.CreateBlockedUser();
        var token = UserTestData.CreateRefreshToken(user);

        var act = () => user.ValidateRefreshToken(token.RefreshToken);

        act.ThrowsException(UserErrorCodes.UserIsBlocked);
    }

    [Fact]
    public void ValidateRefreshToken_ShouldReturnFalse_WhenTokenIsNotFound()
    {
        var user = UserTestData.CreateUser();

        var isValid = user.ValidateRefreshToken(Guid.CreateVersion7());

        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateRefreshToken_ShouldReturnFalse_WhenTokenIsRevoked()
    {
        var user = UserTestData.CreateUser();
        var token = UserTestData.CreateRevokedRefreshToken(user);

        var isValid = user.ValidateRefreshToken(token.RefreshToken);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateRefreshToken_ShouldReturnFalse_WhenTokenIsExpired()
    {
        var user = UserTestData.CreateUser();
        var token = UserTestData.CreateExpiredRefreshToken(user);

        var isValid = user.ValidateRefreshToken(token.RefreshToken);

        isValid.Should().BeFalse();
    }

    #endregion

    #region Revoke

    [Fact]
    public void RevokeRefreshToken_ShouldRevokeRefreshToken_WhenRefreshTokenForTheDeviceIsNotRevoked()
    {
        var user = UserTestData.CreateUser();

        var deviceGuid = Guid.CreateVersion7();

        UserTestData.CreateRefreshToken(
            user,
            deviceGuid: deviceGuid);

        UserTestData.CreateRefreshToken(user);

        user.RevokeRefreshTokens(deviceGuid);

        var revokedRefreshToken = user
            .RefreshTokens
            .Where(rt => rt.DeviceGuid == deviceGuid)
            .FirstOrDefault();

        var notRevokedRefreshToken = user
            .RefreshTokens
            .Where(rt => rt.DeviceGuid != deviceGuid)
            .FirstOrDefault();

        revokedRefreshToken.Should().NotBeNull();
        revokedRefreshToken.Revoked.Should().BeTrue();
        revokedRefreshToken.RevokedDate.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(1));

        notRevokedRefreshToken.Should().NotBeNull();
        notRevokedRefreshToken.Revoked.Should().BeFalse();
        revokedRefreshToken.RevokedDate.Should().NotBeNull();
    }

    #endregion
}
