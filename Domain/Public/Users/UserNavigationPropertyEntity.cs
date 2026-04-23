using Domain.Institutes;
using Domain.System.RefreshToken;

using SLAIS.Domain.Commom;
using SLAIS.Domain.Users;

namespace Domain.Public.Users;

public abstract class UserNavigationPropertyEntity : BaseIdEntity
{
    public ICollection<UserEntity> CreatedUsers { get; private set; } = new List<UserEntity>();

    //Navigation Property for updated Users
    public ICollection<UserEntity> UpdatedUsers { get; private set; } = new List<UserEntity>();

    //Navigation Property for deleted Users
    public ICollection<UserEntity> DeletedUsers { get; private set; } = new List<UserEntity>();

    //Navigation Property for institute
    public InstituteEntity Institute { get; private set; }

    // Navigation Property for created Users
    public ICollection<InstituteEntity> CreatedInstitutes { get; private set; } = new List<InstituteEntity>();

    //Navigation Property for updated Users
    public ICollection<InstituteEntity> UpdatedInstitute { get; private set; } = new List<InstituteEntity>();

    //Navigation Property for deleted Users
    public ICollection<InstituteEntity> DeletedInstitute { get; private set; } = new List<InstituteEntity>();

    //Navigation Property for refresh tokens
    public ICollection<RefreshTokenEntity> RefreshTokens { get; private set; } = new List<RefreshTokenEntity>();

    // Navigation Property for created Users
    protected UserNavigationPropertyEntity(
        Guid? createdByUserGuid) : base(createdByUserGuid)
    {
    }
}
