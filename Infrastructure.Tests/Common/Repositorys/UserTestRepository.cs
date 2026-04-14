using System.Net;

using Domain.System.RefreshToken;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys;

using Microsoft.EntityFrameworkCore;

using SLAIS.Domain.Users;

namespace Infrastructure.Tests.Common.Repositorys;

public class UserTestRepository
{
    public readonly UserRepository UserRepository;

    private readonly SlaisDbContext _dbContext;

    public UserTestRepository(PostgreSqlContainerFixture fixture)
    {
        _dbContext = fixture.SlaisDbContext;
        UserRepository = new UserRepository(_dbContext);
    }

    public async Task<UserEntity> CreateTeacherAsync(Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string? username = null,
        string? password = null)
    {
        var user = UserEntity.CreateTeacher(
            createdByUserGuid,
            email ?? Guid.CreateVersion7() + "@test.com",
            username ?? Guid.CreateVersion7().ToString(),
            firstName ?? "Max",
            lastName ?? "Mustermann",
            instituteGuid);

        await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task CreateStudentAsync(Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string? username = null,
        string? password = null)
    {
        var user = UserEntity.CreateStudent(
            createdByUserGuid,
            email ?? Guid.CreateVersion7() + "@test.com",
            username ?? Guid.CreateVersion7().ToString(),
            firstName ?? "Max",
            lastName ?? "Mustermann",
            instituteGuid);

        await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<UserEntity> CreateAdminAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string? username = null,
        string? password = null)
    {
        var user = UserEntity.CreateAdmin(
            createdByUserGuid,
            email ?? Guid.CreateVersion7() + "@test.com",
            username ?? Guid.CreateVersion7().ToString(),
            firstName ?? "Max",
            lastName ?? "Mustermann",
            instituteGuid);

        user = await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task CreateRefreshTokenForUserAsync(UserEntity user,
        IPAddress? ipAddress = null,
        int? expiresInDays = null,
        Guid? deviceGuid = null,
        string? deviceName = null)
    {
        user.CreateRefreshToken(
            expiresInDays ?? 7,
            deviceGuid ?? Guid.CreateVersion7(),
            deviceName ?? "Test Device",
            ipAddress ?? IPAddress.Loopback);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<RefreshTokenEntity?> GetRefreshTokenByUserGuidAsync(Guid userGuid)
    {
        return await _dbContext
            .GetNoTrackingSet<RefreshTokenEntity>()
            .FirstOrDefaultAsync(rt => rt.UserGuid == userGuid);
    }

    public async Task<UserEntity?> GetUserWithRefreshTokensByGuidAsync(Guid userGuid)
    {
        return await _dbContext
            .GetNoTrackingSet<UserEntity>()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Guid == userGuid);
    }
}
