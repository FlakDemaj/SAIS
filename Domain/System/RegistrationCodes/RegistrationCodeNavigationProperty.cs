using Domain.Base;
using Domain.Public.Users;

using SLAIS.Domain.Commom;

namespace Domain.System.RegistrationCodes;

public class RegistrationCodeNavigationProperty : BaseGuidEntity
{
    // Navigation Property
    public UserEntity User { get; private set; }
}
