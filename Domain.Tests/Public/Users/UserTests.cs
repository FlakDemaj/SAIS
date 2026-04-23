using Domain.Common.Enums;
using Domain.Public.Users;
using Domain.Tests.Common.TestDataCreator;
using Domain.Tests.Utils.Extensions;

using FluentAssertions;

using SLAIS.Domain.Users;

using Xunit;

namespace Domain.Tests.Public.Users;

public class UserTests
{
    #region Create

    [Fact]
    public void CreateAdmin_ShouldSetCorrectEmail()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Email.Should().Be("test@slais.de");
    }

    [Fact]
    public void CreateAdmin_ShouldSetCorrectName()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.FirstName.Should().Be("Max");
        user.LastName.Should().Be("Mustermann");
    }

    [Fact]
    public void CreateAdmin_ShouldSetCorrectPassword()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.HashedPassword.Should().BeNull();
    }

    [Fact]
    public void CreateAdmin_ShouldSetCorrectUserName()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Username.Should().Be("testAdmin");
    }

    [Fact]
    public void CreateSuperAdmin_ShouldSetCorrectRole()
    {
        var user = UserEntity.CreateSuperAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Role.Should().Be(Roles.SuperAdmin);
    }

    [Fact]
    public void CreateAdmin_ShouldSetCorrectRole()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public void CreateTeacher_ShouldSetCorrectRole()
    {
        var user = UserEntity.CreateTeacher(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Role.Should().Be(Roles.Teacher);
    }

    [Fact]
    public void CreateStudent_ShouldSetCorrectRole()
    {
        var user = UserEntity.CreateStudent(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Role.Should().Be(Roles.Student);
    }

    [Fact]
    public void CreateAdmin_ShouldSetDefaultValues()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.LoginAttempts.Should().Be(0);
        user.IsBlocked.Should().BeFalse();
        user.State.Should().Be(States.Active);
    }

    [Fact]
    public void CreateAdmin_ShouldSetAuditDefaults()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.CreatedByUserGuid.Should().NotBeNull();
        user.CreatedDate.Should().BeAfter(DateTime.MinValue);
        user.UpdateDate.Should().BeNull();
        user.UpdatedByUserGuid.Should().BeNull();
        user.DeleteDate.Should().BeNull();
        user.DeletedByUserGuid.Should().BeNull();
    }

    [Fact]
    public void CreateAdmin_ShouldHaveInstituteGuid()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.InstituteGuid.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateAdmin_ShouldGenerateGuid()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Guid.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateAdmin_ShouldNotGenerateId()
    {
        var user = UserEntity.CreateAdmin(
            Guid.CreateVersion7(),
            "test@slais.de",
            "testAdmin",
            "Max",
            "Mustermann",
            Guid.CreateVersion7());

        user.Id.Should().BeNull();
    }

    [Fact]
    public void CreateUser_ShouldThrowException_WhenEmailIsInvalid()
    {
        var act = () => UserEntity.CreateAdmin(
                        Guid.CreateVersion7(),
                        "test",
                        "testAdmin",
                        "Max",
                        "Mustermann",
                        Guid.CreateVersion7());

        act.ThrowsException(UserErrorCodes.InvalidInput);
    }

    [Fact]
    public void CreateUser_ShouldThrowException_WhenUsernameIsWhitespace()
    {
        var act = () => UserEntity.CreateAdmin(
                        Guid.CreateVersion7(),
                        "test@slais.de",
                        " ",
                        "Max",
                        "Mustermann",
                        Guid.CreateVersion7());

        act.ThrowsException(UserErrorCodes.InvalidInput);
    }

    [Fact]
    public void CreateUser_ShouldThrowException_WhenUsernameIsToSmall()
    {
        var act = () => UserEntity.CreateAdmin(
                        Guid.CreateVersion7(),
                        "test@slais.de",
                        "a",
                        "Max",
                        "Mustermann",
                        Guid.CreateVersion7());

        act.ThrowsException(UserErrorCodes.InvalidInput);
    }

    #endregion

    #region Set

    [Fact]
    public void SetPassword_ShouldSetCorrectPassword()
    {
        var user = UserTestData.CreateUser();
        var newPassword = "NewPassword";

        user.SetPassword(newPassword);

        user.HashedPassword.Should().Be(newPassword);
    }

    [Fact]
    public void SetPassword_ShouldThrowException_WhenPasswordIsInvalid()
    {
        var user = UserTestData.CreateUser();

        var act = () => user.SetPassword(" ");

        act.ThrowsException(UserErrorCodes.InvalidPassword);
    }

    [Fact]
    public void SetPassword_ShouldThrowException_PasswordIsTheSame()
    {
        var user = UserTestData.CreateUser();
        var newPassword = "NewPassword";

        var act = () => user.SetPassword(newPassword);

        act.ThrowsException(UserErrorCodes.InvalidPassword);
    }

    [Fact]
    public void IncrementWrongLoginAttempts_ShouldIncrementLoginAttempts()
    {
        var user = UserTestData.CreateUser();

        user.IncrementWrongLoginAttempts();

        user.LoginAttempts.Should().Be(1);
        user.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void IncrementWrongLoginAttempts_ShouldBlockUser()
    {
        var user = UserTestData.CreateUserWithLoginAttempts(4);

        user.IncrementWrongLoginAttempts();

        user.LoginAttempts.Should().Be(5);
        user.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void IncrementWrongLoginAttempts_ShouldThrowException_UserAlreadyBlocked()
    {
        var user = UserTestData.CreateBlockedUser();

        var act = () => user.IncrementWrongLoginAttempts();

        act.ThrowsException(UserErrorCodes.UserIsBlocked);
    }

    [Fact]
    public void SetLoginAttemptsToZero_ShouldSetCorrectLoginAttempts()
    {
        var user = UserTestData.CreateUserWithLoginAttempts(3);

        user.IncrementWrongLoginAttempts();

        user.SetLoginAttemptsToZero();

        user.LoginAttempts.Should().Be(0);
    }

    #endregion
}
