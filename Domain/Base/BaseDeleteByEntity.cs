using Domain.Public.Users;

namespace SLAIS.Domain.Commom;

public abstract class BaseDeleteByEntity : BaseUpdatedByEntity
{
    public Guid? DeletedByUserGuid { get; private set; }

    public DateTime? DeleteDate { get; private set; }

    // Navigation property
    public UserEntity? DeletedByUser { get; private set; }

    protected BaseDeleteByEntity(
        Guid? createdByUserGuid)
        : base(createdByUserGuid)
    {
        DeletedByUserGuid = null;
        DeleteDate = null;
    }

}
