using Domain.Public.Users;

using SLAIS.Domain.Commom;

namespace Domain.Institutes;

public abstract class InstituteNavigationPropertyEntity : BaseIdEntity
{
    //Navigation Property to Users
    public ICollection<UserEntity> Users { get; private set; } = new List<UserEntity>();

    protected InstituteNavigationPropertyEntity
        (Guid? createdByUserGuid)
        : base(createdByUserGuid)
    {
    }
}
