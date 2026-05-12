using Domain.Common.Enums;
using Domain.Public.Users;
using Domain.System.RefreshToken;

using Tests.Domain.Shared.Builders;

namespace Tests.Domain.Shared.TestDataCreator;

public static class UserTestData
{
    public static UserEntity CreateUser(Roles roles = Roles.Admin)
    {
        return new UserEntityBuilder().WithRole(roles).Build();
    }

    public static UserEntity CreateBlockedUser()
    {
        return new UserEntityBuilder().Blocked().Build();
    }

    public static UserEntity CreateUserWithLoginAttempts(int count)
    {
        return new UserEntityBuilder().WithLoginAttempts((short)count).Build();
    }

    public static RefreshTokenEntity CreateRefreshToken(
        UserEntity user,
        int expiresInDays = 7,
        Guid? deviceGuid = null,
        string? deviceName = null)
    {
        return RefreshTokenEntityBuilder.CreateValid(user, expiresInDays, deviceGuid, deviceName);
    }

    public static RefreshTokenEntity CreateExpiredRefreshToken(UserEntity user)
    {
        return RefreshTokenEntityBuilder.CreateExpired(user);
    }

    public static RefreshTokenEntity CreateRevokedRefreshToken(UserEntity user)
    {
        return RefreshTokenEntityBuilder.CreateRevoked(user);
    }
}
