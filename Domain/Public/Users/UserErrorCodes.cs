using System.ComponentModel;

namespace Domain.Public.Users;

public enum UserErrorCodes
{
    [Description("Der Nutzer ist blockiert.")]
    UserIsBlocked = -310001,

    [Description("Das Password ist nicht erlaubt.")]
    InvalidPassword = -310002,

    [Description("Leider sind die angegebenen Daten nicht erlaubt.")]
    InvalidInput = -310003,

    [Description("Der Nutzer wurde noch nicht aktiviert. Bitte gucken Sie in Ihren E-Mail Postfach.")]
    UserIsNotActivated = -310004,

    [Description("Der Nutzer wurde ist deaktivert. Bitte wenden Sie sich an Ihren Vorgesetzten.")]
    UserIsDeactivated = -310005,

    [Description("Der Nutzer wurde wurde gelöscht. Bitte wenden Sie sich an Ihren Vorgesetzten.")]
    UserIsDeleted = -310006,
}
