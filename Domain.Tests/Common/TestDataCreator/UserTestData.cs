using System.Net;
using System.Reflection;

using Domain.Common.Enums;
using Domain.System.RefreshToken;

using SLAIS.Domain.Users;

namespace Domain.Tests.Common.TestDataCreator;

public static class UserTestData
{
    public static UserEntity CreateUser(Roles roles = Roles.Admin)
    {
        var user = UserEntity.CreateAdmin(
            null,
            "test@test.com",
            "testuser",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        SetProperty(user, "State", States.Active);

        return user;
    }

    public static UserEntity CreateBlockedUser()
    {
        var user = CreateUser();
        SetProperty(user, "IsBlocked", true);
        return user;
    }

    public static UserEntity CreateUserWithLoginAttempts(int count)
    {
        var user = CreateUser();
        SetProperty(user, "LoginAttempts", (short)count);
        return user;
    }

    public static RefreshTokenEntity CreateRefreshToken(
        UserEntity user,
        int expiresInDays = 7,
        Guid? deviceGuid = null,
        string? deviceName = null)
    {
        return user.CreateRefreshToken(
            expiresInDays,
            deviceGuid ?? Guid.CreateVersion7(),
            deviceName ?? "Test Device",
            IPAddress.Loopback);
    }

    public static RefreshTokenEntity CreateExpiredRefreshToken(UserEntity user)
    {
        var token = user.CreateRefreshToken(
            1,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        SetProperty(token, "ExpirationDate", DateTime.UtcNow.AddDays(-1));

        return token;
    }

    public static RefreshTokenEntity CreateRevokedRefreshToken(UserEntity user)
    {
        var token = user.CreateRefreshToken(
            7,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        SetProperty(token, "Revoked", true);
        SetProperty(token, "RevokedDate", (DateTime?)DateTime.UtcNow);

        return token;
    }

    private static void SetProperty<T>(T obj, string propertyName, object? value) where T : class
    {
        var type = obj.GetType();
        var prop = type.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        prop?.SetValue(obj, value);
    }
}
