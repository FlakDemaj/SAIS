using Domain.Common.Enums;
using Domain.Public.Users;

namespace Application.Common.Interfaces.Repositorys;

public interface IUserRepository : IBaseRepository<UserEntity>
{
    Task<UserEntity?> GetUserByGuidAsync(Guid userGuid, Guid instituteGuid);

    Task<List<UserEntity>> GetAllUsersFromInstituteAsync(
        Guid instituteGuid,
        Roles userRole);

    Task<UserEntity?> GetUserByUsernameOrEmailWithRefreshTokenAsync(string username);

    Task<UserEntity?> GetUserWithRefreshTokensByGuidAsync(Guid refreshTokenGuid);

    Task<List<string>> GetUsernamesByPrefixAsync(
        string usernamePrefix);

}
