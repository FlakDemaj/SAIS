using System.Security.Cryptography;

namespace Domain.System.RegistrationCodes;

public class RegistrationCodeEntity : RegistrationCodeNavigationProperty
{
    public string RegistrationCode { get; private set; }

    public bool Revoked { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid UserGuid { get; private set; }

    private RegistrationCodeEntity()
    {
    }

    private RegistrationCodeEntity(
        string registrationCode,
        Guid userGuid)
    {
        RegistrationCode = registrationCode;
        Revoked = false;
        CreatedAt = DateTime.UtcNow;
        UserGuid = userGuid;
    }

    public static RegistrationCodeEntity Create(
        Guid userGuid)
    {
        var code = GenerateCode();

        return new RegistrationCodeEntity(
            code,
            userGuid);
    }

    private static string GenerateCode()
    {
        var bytes = new byte[6];
        RandomNumberGenerator.Fill(bytes);
        var value = BitConverter.ToInt32(bytes, 0) & 0x7fffffff;
        return (value % 900000 + 100000).ToString();
    }


}
