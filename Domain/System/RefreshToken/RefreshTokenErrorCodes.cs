using System.ComponentModel;

namespace Domain.System.RefreshToken;

public enum RefreshTokenErrorCodes
{
    [Description("Irgendwas lief falsch. Bitte versuchen Sie es bald erneut.")]
    InvalidExpiration = -410001,

    [Description("Irgendwas lief falsch. Bitte versuchen Sie es bald erneut.")]
    InvalidDeviceGuid = -410002,

    [Description("Irgendwas lief falsch. Bitte versuchen Sie es bald erneut.")]
    InvalidDeviceName = -410003,
}
