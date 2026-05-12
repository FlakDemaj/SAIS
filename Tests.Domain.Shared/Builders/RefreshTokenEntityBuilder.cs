using System.Net;
using System.Reflection;

using Domain.Public.Users;
using Domain.System.RefreshToken;

namespace Tests.Domain.Shared.Builders;

public static class RefreshTokenEntityBuilder
{
    public static RefreshTokenEntity CreateValid(
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

    public static RefreshTokenEntity CreateExpired(UserEntity user)
    {
        var token = user.CreateRefreshToken(
            1,
            Guid.CreateVersion7(),
            "Test Device",
            IPAddress.Loopback);

        SetProperty(token, "ExpirationDate", DateTime.UtcNow.AddDays(-1));

        return token;
    }

    public static RefreshTokenEntity CreateRevoked(UserEntity user)
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
        var prop = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        prop?.SetValue(obj, value);
    }
}
