using System.Net;

using Domain.Common.Enums;
using Domain.Common.Exceptions;
using Domain.Public.Users;
using Domain.System.RefreshToken;

namespace SLAIS.Domain.Users;

public class UserEntity : UserNavigationPropertyEntity
{
    public string Email { get; private set; }

    public string? HashedPassword { get; private set; }

    public string Username { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public Roles Role { get; private set; }

    public short LoginAttempts { get; private set; }

    public bool IsBlocked { get; private set; }

    public States State { get; private set; }

    public Guid InstituteGuid { get; private set; }

    private UserEntity(
        Guid? createdByUserGuid,
        string email,
        string username,
        string firstName,
        string lastName,
        Roles role,
        Guid instituteGuid)
        : base(createdByUserGuid)
    {
        Email = email;
        HashedPassword = null;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        LoginAttempts = 0;
        IsBlocked = false;
        State = States.Pending;
        InstituteGuid = instituteGuid;
    }

    public static UserEntity CreateAdmin(
        Guid? createdByUserGuid,
        string email,
        string username,
        string firstName,
        string lastName,
        Guid instituteGuid)
    {
        CheckInputs(
            email,
            username,
            firstName,
            lastName);

        return new UserEntity(
            createdByUserGuid,
            email,
            username,
            firstName,
            lastName,
            Roles.Admin,
            instituteGuid
        );
    }

    public static UserEntity CreateTeacher(
        Guid? createdByUserGuid,
        string email,
        string username,
        string firstName,
        string lastName,
        Guid instituteGuid)
    {
        CheckInputs(
            email,
            username,
            firstName,
            lastName);

        return new UserEntity(
            createdByUserGuid,
            email,
            username,
            firstName,
            lastName,
            Roles.Teacher,
            instituteGuid
        );
    }

    public static UserEntity CreateStudent(
        Guid? createdByUserGuid,
        string email,
        string username,
        string firstName,
        string lastName,
        Guid instituteGuid)
    {
        CheckInputs(
            email,
            username,
            firstName,
            lastName);

        return new UserEntity(
            createdByUserGuid,
            email,
            username,
            firstName,
            lastName,
            Roles.Student,
            instituteGuid
        );
    }

    public static UserEntity CreateSuperAdmin(
        Guid? createdByUserGuid,
        string email,
        string username,
        string firstName,
        string lastName,
        Guid instituteGuid)
    {
        CheckInputs(
            email,
            username,
            firstName,
            lastName);

        return new UserEntity(
            createdByUserGuid,
            email,
            username,
            firstName,
            lastName,
            Roles.SuperAdmin,
            instituteGuid
        );
    }

    public void IncrementWrongLoginAttempts(int maxLoginAttempts = 5)
    {
        CheckUser();

        LoginAttempts++;

        if (LoginAttempts == maxLoginAttempts)
        {
            IsBlocked = true;
        }
    }

    public void SetLoginAttemptsToZero()
    {
        LoginAttempts = 0;
    }

    public void SetPassword(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword)
            || hashedPassword == HashedPassword)
        {
            throw new SlaisException(UserErrorCodes.InvalidPassword);
        }

        HashedPassword = hashedPassword;
    }

    public RefreshTokenEntity CreateRefreshToken(
        int expiresInDays,
        Guid deviceGuid,
        string deviceName,
        IPAddress ipAddress)
    {
        var refreshToken = RefreshTokenEntity.CreateRefreshToken(
            expiresInDays,
            deviceGuid,
            deviceName,
            ipAddress,
            Guid);

        RefreshTokens.Add(refreshToken);

        return refreshToken;
    }

    public bool ValidateRefreshToken(Guid refreshTokenGuid)
    {
        CheckUser();

        var refreshToken = RefreshTokens
            .FirstOrDefault(rt => rt.RefreshToken == refreshTokenGuid);

        return refreshToken != null && refreshToken.Validate();
    }

    public void RevokeRefreshTokens(Guid deviceGuid)
    {
        var activeRefreshTokensInTheDevice =
            RefreshTokens
                .Where(rt => rt.DeviceGuid == deviceGuid
                                                 && !rt.Revoked)
                .ToList();

        foreach (var refreshToken in activeRefreshTokensInTheDevice)
        {
            refreshToken.Revoke();
        }
    }

    #region Private

    private static void CheckInputs(
        string email,
        string username,
        string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(email)
            || !email.Contains('@')
            || email.Length < 6)
        {
            throw new SlaisException(UserErrorCodes.InvalidInput);
        }

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName))
        {
            throw new SlaisException(UserErrorCodes.InvalidInput);
        }

        if (username.Length < 3
            || username.Length > 100)
        {
            throw new SlaisException(UserErrorCodes.InvalidInput);
        }
    }

    public void CheckUser()
    {
        if (IsBlocked)
        {
            throw new SlaisException(UserErrorCodes.UserIsBlocked);
        }

        switch (State)
        {
            case States.Pending:
                throw new SlaisException(UserErrorCodes.UserIsNotActivated);
            case States.Deactived:
                throw new SlaisException(UserErrorCodes.UserIsDeactivated);
            case States.Deleted:
                throw new SlaisException(UserErrorCodes.UserIsDeleted);
            case States.Active:
                return;
            default:
                throw new SlaisException(UserErrorCodes.InvalidInput);
        }
    }

    #endregion
}
