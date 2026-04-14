using System.Reflection;
using System.Security.Cryptography;

using Domain.Common.Enums;
using Domain.System.RefreshToken;

using SLAIS.Domain.Users;

namespace Tests.Shared.Builders;

public class UserEntityBuilder
{
    private Guid? _createdByUserGuid;
    private string _email = "test@slais.de";
    private string _hashedPassword = "HashedPassword";
    private string _username = "testAdmin";
    private string _firstName = "Max";
    private string _lastName = "Mustermann";
    private Guid _instituteGuid = Guid.NewGuid();
    private bool _isBlocked;
    private short _loginAttempts;
    private Roles _role = Roles.Admin;
    private readonly States _state = States.Active;
    private UserEntity? _createdByUser;

    #region With Methods

    public UserEntityBuilder WithCreatedByUserGuid(UserEntity? createdByUser)
    {
        if (createdByUser == null)
        {
            return this;
        }

        _createdByUserGuid = createdByUser.Guid;
        _createdByUser = createdByUser;
        return this;
    }

    public UserEntityBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserEntityBuilder WithHashedPassword(string hashedPassword)
    {
        _hashedPassword = hashedPassword;
        return this;
    }

    public UserEntityBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserEntityBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public UserEntityBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public UserEntityBuilder WithInstituteGuid(Guid instituteGuid)
    {
        _instituteGuid = instituteGuid;
        return this;
    }

    public UserEntityBuilder WithBlocked()
    {
        _isBlocked = true;
        return this;
    }

    public UserEntityBuilder WithLoginAttempts(short loginAttempts)
    {
        _loginAttempts = loginAttempts;
        return this;
    }

    public UserEntityBuilder WithRole(Roles role)
    {
        _role = role;
        return this;
    }

    #endregion

    #region Build

    public UserEntity Build()
    {
        var user = CreateEmptyInstance();

        SetProperty(user, nameof(UserEntity.Email), _email);
        SetProperty(user, nameof(UserEntity.HashedPassword), _hashedPassword);
        SetProperty(user, nameof(UserEntity.Username), _username);
        SetProperty(user, nameof(UserEntity.FirstName), _firstName);
        SetProperty(user, nameof(UserEntity.LastName), _lastName);
        SetProperty(user, nameof(UserEntity.InstituteGuid), _instituteGuid);
        SetProperty(user, nameof(UserEntity.IsBlocked), _isBlocked);
        SetProperty(user, nameof(UserEntity.LoginAttempts), _loginAttempts);
        SetProperty(user, nameof(UserEntity.Role), _role);
        SetProperty(user, nameof(UserEntity.State), _state);
        SetProperty(user, nameof(UserEntity.CreatedByUserGuid), _createdByUserGuid);
        SetProperty(user, nameof(UserEntity.CreatedByUser), _createdByUser);
        SetProperty(user, nameof(UserEntity.Id), RandomNumberGenerator.GetInt32(1, int.MaxValue));

        InitializeCollection(user);

        return user;
    }

    #endregion

    #region Private

    private static UserEntity CreateEmptyInstance()
    {
        return (UserEntity)System.Runtime.CompilerServices
            .RuntimeHelpers
            .GetUninitializedObject(typeof(UserEntity));
    }

    private static void SetProperty<TValue>(
        UserEntity entity,
        string propertyName,
        TValue value)
    {
        var type = typeof(UserEntity);

        while (type != null)
        {

#pragma warning disable S3011
            var property = type.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#pragma warning restore S3011

            if (property != null)
            {
                property.SetValue(entity, value);
                return;
            }

            type = type.BaseType;
        }
    }

    private static void InitializeCollection(UserEntity user)
    {
        var type = typeof(UserEntity);

        while (type != null)
        {
#pragma warning disable S3011
            var field = type.GetFields(
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .FirstOrDefault(f =>
                {
                    return f.FieldType == typeof(ICollection<RefreshTokenEntity>);
                });
#pragma warning restore S3011

            if (field != null)
            {
                field.SetValue(user, new List<RefreshTokenEntity>());
                return;
            }

            type = type.BaseType;
        }
    }

    #endregion
}
