using Domain.Public.Users;

using SLAIS.Domain.Commom;

namespace Domain.Base;

public abstract class BaseCreatedByEntity : BaseGuidEntity
{
    public Guid? CreatedByUserGuid { get; private set; }

    public DateTime CreatedDate { get; init; }

    // Navigation property
    public UserEntity? CreatedByUser { get; init; }

    protected BaseCreatedByEntity(
        Guid? createdByUserGuid)
    {
        CreatedByUserGuid = createdByUserGuid;
        CreatedDate = DateTime.UtcNow;
    }

}
