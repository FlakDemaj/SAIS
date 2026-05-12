using Domain.Public.Users;

using SLAIS.Domain.Commom;

namespace Domain.System.RefreshToken;

public abstract class RefreshTokenNavigationPropertyEntity : BaseGuidEntity
{
    //Navigation Property for user
    public UserEntity User { get; private set; }

}
