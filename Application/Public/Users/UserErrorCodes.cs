using System.ComponentModel;

namespace Application.Public.Users;

public enum UserErrorCodes
{
    [Description("An user with this Id was not found.")]
    UserNotFound = -300001,

    [Description("Ein Nutzer mit dieser Email existiert schon.")]
    UserWithThisEmailAlreadyExists = -300002,

    [Description("Sie haben keinen Zugriff auf diese Funktion.")]
    Forbidden = -300003
}
