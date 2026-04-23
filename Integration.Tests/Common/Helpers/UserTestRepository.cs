using System.Net;

using Application.Common.Interfaces.Services;

using Domain.System.RefreshToken;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys;

using Microsoft.EntityFrameworkCore;

using SLAIS.Domain.Users;

namespace Integration.Tests.Common.Helpers;

public class UserTestRepository
{
    public readonly UserRepository UserRepository;

    private readonly SlaisDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserTestRepository(SlaisDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        UserRepository = new UserRepository(dbContext);
    }

    public async Task<UserEntity> CreateAdminAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null)
    {
        var user = UserEntity.CreateAdmin(
            createdByUserGuid,
            email ?? $"{Guid.CreateVersion7()}@test.com",
            username ?? Guid.CreateVersion7().ToString(),
            firstName ?? "Max",
            lastName ?? "Mustermann",
            instituteGuid);

        user = await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntity> CreateAdminWithPasswordAsync(
        Guid instituteGuid,
        string rawPassword,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null)
    {
        var user = UserEntity.CreateAdmin(
            createdByUserGuid,
            email ?? $"{Guid.CreateVersion7()}@test.com",
            username ?? Guid.CreateVersion7().ToString(),
            firstName ?? "Max",
            lastName ?? "Mustermann",
            instituteGuid);

        user.SetPassword(_passwordHasher.Hash(rawPassword));

        user = await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();
        await ActivateUserAsync(user.Guid);

        return user;
    }

    public async Task<UserEntity> CreateTeacherAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null)
    {
        var user = UserEntity.CreateTeacher(
            createdByUserGuid,
            email ?? $"{Guid.CreateVersion7()}@test.com",
            username ?? Guid.CreateVersion7().ToString(),
            firstName ?? "Max",
            lastName ?? "Mustermann",
            instituteGuid);

        user = await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntity> CreateStudentAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null)
    {
        var user = UserEntity.CreateStudent(
            createdByUserGuid,
            email ?? $"{Guid.CreateVersion7()}@test.com",
            username ?? Guid.CreateVersion7().ToString(),
            firstName ?? "Max",
            lastName ?? "Mustermann",
            instituteGuid);

        user = await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<RefreshTokenEntity> CreateRefreshTokenForUserAsync(
        UserEntity user,
        IPAddress? ipAddress = null,
        int? expiresInDays = null,
        Guid? deviceGuid = null,
        string? deviceName = null)
    {
        var refreshToken = user.CreateRefreshToken(
            expiresInDays ?? 7,
            deviceGuid ?? Guid.CreateVersion7(),
            deviceName ?? "Test Device",
            ipAddress ?? IPAddress.Loopback);

        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshTokenEntity?> GetRefreshTokenByUserGuidAsync(Guid userGuid)
    {
        return await _dbContext
            .GetNoTrackingSet<RefreshTokenEntity>()
            .FirstOrDefaultAsync(rt => rt.UserGuid == userGuid);
    }

    private async Task ActivateUserAsync(Guid userGuid)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"UPDATE \"public\".\"users\" SET \"state\" = 0 WHERE \"user_guid\" = {userGuid}");
    }
}
