using System.Reflection;

using Domain.Common.Enums;

using SLAIS.Domain.Users;

namespace Tests.Domain.Shared.Builders;

public class UserEntityBuilder
{
    private Roles _role = Roles.Admin;
    private States _state = States.Active;
    private bool _isBlocked = false;
    private short? _loginAttempts = null;
    private string? _hashedPassword = null;
    private string _email;
    private string _username;
    private string _firstName = "Max";
    private string _lastName = "Mustermann";
    private Guid _instituteGuid;
    private Guid? _createdByUserGuid = null;

    public UserEntityBuilder()
    {
        _email = $"{Guid.CreateVersion7()}@test.com";
        _username = Guid.CreateVersion7().ToString();
        _instituteGuid = Guid.CreateVersion7();
    }

    public UserEntityBuilder WithRole(Roles role) { _role = role; return this; }
    public UserEntityBuilder WithState(States state) { _state = state; return this; }
    public UserEntityBuilder Blocked() { _isBlocked = true; return this; }
    public UserEntityBuilder WithLoginAttempts(short count) { _loginAttempts = count; return this; }
    public UserEntityBuilder WithHashedPassword(string hashedPassword) { _hashedPassword = hashedPassword; return this; }
    public UserEntityBuilder WithEmail(string email) { _email = email; return this; }
    public UserEntityBuilder WithUsername(string username) { _username = username; return this; }
    public UserEntityBuilder WithFirstName(string firstName) { _firstName = firstName; return this; }
    public UserEntityBuilder WithLastName(string lastName) { _lastName = lastName; return this; }
    public UserEntityBuilder WithInstituteGuid(Guid guid) { _instituteGuid = guid; return this; }
    public UserEntityBuilder WithCreatedBy(Guid guid) { _createdByUserGuid = guid; return this; }

    public UserEntity Build()
    {
        var user = _role switch
        {
            Roles.Admin => UserEntity.CreateAdmin(
                _createdByUserGuid, _email, _username, _firstName, _lastName, _instituteGuid),
            Roles.Teacher => UserEntity.CreateTeacher(
                _createdByUserGuid, _email, _username, _firstName, _lastName, _instituteGuid),
            Roles.Student => UserEntity.CreateStudent(
                _createdByUserGuid, _email, _username, _firstName, _lastName, _instituteGuid),
            Roles.SuperAdmin => UserEntity.CreateSuperAdmin(
                _createdByUserGuid, _email, _username, _firstName, _lastName, _instituteGuid),
            _ => throw new ArgumentException($"Unsupported role: {_role}")
        };

        SetProperty(user, "State", _state);

        if (_isBlocked)
        {
            SetProperty(user, "IsBlocked", true);
        }

        if (_loginAttempts.HasValue)
        {
            SetProperty(user, "LoginAttempts", _loginAttempts.Value);
        }

        if (_hashedPassword is not null)
        {
            SetProperty(user, "HashedPassword", _hashedPassword);
        }

        return user;
    }

    private static void SetProperty<T>(T obj, string propertyName, object? value) where T : class
    {
        var prop = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        prop?.SetValue(obj, value);
    }
}
