using Application.Common.Interfaces.Repositorys;

using Domain.Common.Enums;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys.Base;

using Microsoft.EntityFrameworkCore;

using SLAIS.Domain.Users;

namespace Infrastructure.Repositorys;

public class UserRepository : BaseRepository<UserEntity>, IUserRepository
{
    public UserRepository(SlaisDbContext context)
        : base(context)
    {
    }

    public Task<UserEntity?> GetUserByGuidAsync(Guid userGuid, Guid instituteGuid)
    {
        return _context
               .GetTrackingSet<UserEntity>()
               .Include(user => user.CreatedByUser)
               .Include(user => user.UpdatedByUser)
               .Include(user => user.DeletedByUser)
               .FirstOrDefaultAsync(user => user.Guid == userGuid
               && user.InstituteGuid == instituteGuid);
    }

    public async Task<List<UserEntity>> GetAllUsersFromInstituteAsync(Guid instituteGuid, Roles userRole)
    {
        var query = _context
            .GetNoTrackingSet<UserEntity>()
            .Where(user => user.InstituteGuid == instituteGuid);

        query = ApplyRoleFilter(query, userRole);

        return await query
            .ToListAsync();
    }

    public Task<UserEntity?> GetUserByUsernameOrEmailWithRefreshTokenAsync(string username)
    {
        return _context
            .GetTrackingSet<UserEntity>()
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Email == username
                                         || user.Username == username);
    }

    public async Task<UserEntity?> GetUserWithRefreshTokensByGuidAsync(Guid refreshTokenGuid)
    {
        return await _context
            .GetTrackingSet<UserEntity>()
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.RefreshTokens
                .Any(rt => rt.RefreshToken == refreshTokenGuid));
    }

    public async Task<List<string>> GetUsernamesByPrefixAsync(
        string usernamePrefix)
    {
        return await _context
            .GetNoTrackingSet<UserEntity>()
            .Where(user => user.Username.StartsWith(usernamePrefix))
            .Select(user => user.Username)
            .ToListAsync();
    }

    private static IQueryable<UserEntity> ApplyRoleFilter(
        IQueryable<UserEntity> query, Roles userRole)
    {
        if (userRole == Roles.SuperAdmin || userRole == Roles.Server)
        {
            return query;
        }

        if (userRole == Roles.Admin)
        {
            return query.Where(user => user.Role == Roles.Student
            || user.Role == Roles.Teacher);

        }

        if (userRole == Roles.Teacher)
        {
            return query.Where(user => user.Role == Roles.Student);
        }

        // Fallback. Do Nothing
        return query.Where(user => false);
    }
}
