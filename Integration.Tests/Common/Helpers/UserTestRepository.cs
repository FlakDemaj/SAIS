using System.Net;

using Application.Common.Interfaces.Services;

using Domain.Common.Enums;
using Domain.System.RefreshToken;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys;

using Microsoft.EntityFrameworkCore;

using SLAIS.Domain.Users;

using Tests.Domain.Shared.Builders;

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

    private async Task<UserEntity> SaveUserAsync(UserEntity user)
    {
        user = await UserRepository.CreateAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<UserEntity> CreateAdminAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        States state = States.Active,
        bool isBlocked = false,
        string? password = null)
    {
        var builder = new UserEntityBuilder()
            .WithInstituteGuid(instituteGuid)
            .WithRole(Roles.Admin)
            .WithEmail(email ?? $"{Guid.CreateVersion7()}@test.com")
            .WithUsername(username ?? Guid.CreateVersion7().ToString())
            .WithFirstName(firstName ?? "Max")
            .WithLastName(lastName ?? "Mustermann")
            .WithState(state);

        if (createdByUserGuid.HasValue)
        {
            builder.WithCreatedBy(createdByUserGuid.Value);
        }

        if (isBlocked)
        {
            builder.Blocked();
        }

        if (password is not null)
        {
            builder.WithHashedPassword(_passwordHasher.Hash(password));
        }

        return await SaveUserAsync(builder.Build());
    }

    public async Task<UserEntity> CreateSuperAdminAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        States state = States.Active,
        bool isBlocked = false,
        string? password = null)
    {
        var builder = new UserEntityBuilder()
            .WithInstituteGuid(instituteGuid)
            .WithRole(Roles.SuperAdmin)
            .WithEmail(email ?? $"{Guid.CreateVersion7()}@test.com")
            .WithUsername(username ?? Guid.CreateVersion7().ToString())
            .WithFirstName(firstName ?? "Max")
            .WithLastName(lastName ?? "Mustermann")
            .WithState(state);

        if (createdByUserGuid.HasValue)
        {
            builder.WithCreatedBy(createdByUserGuid.Value);
        }

        if (isBlocked)
        {
            builder.Blocked();
        }

        if (password is not null)
        {
            builder.WithHashedPassword(_passwordHasher.Hash(password));
        }

        return await SaveUserAsync(builder.Build());
    }

    public async Task<UserEntity> CreateTeacherAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        States state = States.Active,
        bool isBlocked = false,
        string? password = null)
    {
        var builder = new UserEntityBuilder()
            .WithInstituteGuid(instituteGuid)
            .WithRole(Roles.Teacher)
            .WithEmail(email ?? $"{Guid.CreateVersion7()}@test.com")
            .WithUsername(username ?? Guid.CreateVersion7().ToString())
            .WithFirstName(firstName ?? "Max")
            .WithLastName(lastName ?? "Mustermann")
            .WithState(state);

        if (createdByUserGuid.HasValue)
        {
            builder.WithCreatedBy(createdByUserGuid.Value);
        }

        if (isBlocked)
        {
            builder.Blocked();
        }

        if (password is not null)
        {
            builder.WithHashedPassword(_passwordHasher.Hash(password));
        }

        return await SaveUserAsync(builder.Build());
    }

    public async Task<UserEntity> CreateStudentAsync(
        Guid instituteGuid,
        Guid? createdByUserGuid = null,
        string? email = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        States state = States.Active,
        bool isBlocked = false,
        string? password = null)
    {
        var builder = new UserEntityBuilder()
            .WithInstituteGuid(instituteGuid)
            .WithRole(Roles.Student)
            .WithEmail(email ?? $"{Guid.CreateVersion7()}@test.com")
            .WithUsername(username ?? Guid.CreateVersion7().ToString())
            .WithFirstName(firstName ?? "Max")
            .WithLastName(lastName ?? "Mustermann")
            .WithState(state);

        if (createdByUserGuid.HasValue)
        {
            builder.WithCreatedBy(createdByUserGuid.Value);
        }

        if (isBlocked)
        {
            builder.Blocked();
        }

        if (password is not null)
        {
            builder.WithHashedPassword(_passwordHasher.Hash(password));
        }

        return await SaveUserAsync(builder.Build());
    }

    public async Task<RefreshTokenEntity> CreateRefreshTokenForUserAsync(
        UserEntity user,
        IPAddress? ipAddress = null,
        int? expiresInDays = null,
        Guid? deviceGuid = null,
        string? deviceName = null)
    {
        var refreshToken = RefreshTokenEntityBuilder.CreateValid(
            user,
            expiresInDays ?? 7,
            deviceGuid,
            deviceName);

        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshTokenEntity> CreateExpiredRefreshTokenForUserAsync(UserEntity user)
    {
        var refreshToken = RefreshTokenEntityBuilder.CreateExpired(user);
        await _dbContext.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshTokenEntity> CreateRevokedRefreshTokenForUserAsync(UserEntity user)
    {
        var refreshToken = RefreshTokenEntityBuilder.CreateRevoked(user);
        await _dbContext.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshTokenEntity?> GetRefreshTokenByUserGuidAsync(Guid userGuid)
    {
        return await _dbContext
            .GetNoTrackingSet<RefreshTokenEntity>()
            .FirstOrDefaultAsync(rt => rt.UserGuid == userGuid);
    }
}
