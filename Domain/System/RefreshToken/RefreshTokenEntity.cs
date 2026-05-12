using System.Net;

using Domain.Common.Exceptions;

namespace Domain.System.RefreshToken;

public class RefreshTokenEntity : RefreshTokenNavigationPropertyEntity
{
    public Guid RefreshToken { get; private set; }

    public DateTime ExpirationDate { get; private set; }

    public Guid DeviceGuid { get; private set; }

    public string DeviceName { get; private set; }

    public IPAddress IpAddress { get; private set; }

    public bool Revoked { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime LastUsedDate { get; private set; }

    public DateTime? RevokedDate { get; private set; }

    public Guid UserGuid { get; private set; }

    //EF Core
    private RefreshTokenEntity()
    {
        DeviceName = null!;
        IpAddress = null!;
    }

    private RefreshTokenEntity(
        int expirationInDays,
        Guid deviceGuid,
        string deviceName,
        IPAddress ipAddress,
        Guid userGuid)
    {

        CheckInput(expirationInDays,
            deviceGuid,
            deviceName);

        RefreshToken = Guid.CreateVersion7();
        ExpirationDate = DateTime.UtcNow.AddDays(expirationInDays);
        DeviceGuid = deviceGuid;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        Revoked = false;
        CreatedDate = DateTime.UtcNow;
        LastUsedDate = DateTime.UtcNow;
        RevokedDate = null;
        UserGuid = userGuid;
    }

    internal static RefreshTokenEntity CreateRefreshToken(int expirationInDays,
        Guid deviceGuid,
        string deviceName,
        IPAddress ipAddress,
        Guid userGuid)
    {
        return new RefreshTokenEntity(
            expirationInDays,
            deviceGuid,
            deviceName,
            ipAddress,
            userGuid);
    }

    public bool Validate()
    {
        if (Revoked)
        {
            return false;
        }

        if (ExpirationDate <= DateTime.UtcNow)
        {
            Revoke();
            return false;
        }

        LastUsedDate = DateTime.UtcNow;
        return true;
    }

    public void Revoke()
    {
        Revoked = true;
        RevokedDate = DateTime.UtcNow;
    }

    public int GetExpirationInDays()
    {
        return ExpirationDate.Subtract(LastUsedDate).Days;
    }

    private static void CheckInput(
        int expirationInDays,
        Guid deviceGuid,
        string deviceName)
    {
        if (expirationInDays <= 0)
        {
            throw new SlaisException(RefreshTokenErrorCodes.InvalidExpiration);
        }

        if (deviceGuid == Guid.Empty)
        {
            throw new SlaisException(RefreshTokenErrorCodes.InvalidDeviceGuid);
        }

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            throw new SlaisException(RefreshTokenErrorCodes.InvalidDeviceName);
        }
    }
}
