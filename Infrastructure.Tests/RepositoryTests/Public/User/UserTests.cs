using Domain.Common.Enums;

using FluentAssertions;

using Infrastructure.Tests.Common;
using Infrastructure.Tests.Common.Repositorys;
using Infrastructure.Tests.Common.Shared;

using SLAIS.Domain.Users;

using Xunit;

namespace Infrastructure.Tests.RepositoryTests.Public.User;

public class UserTests : TestBase
{
    private readonly UserTestRepository _userTestRepository;

    private readonly InstituteTestRepository _instituteTestRepository;

    public UserTests(PostgreSqlContainerFixture fixture)
        : base(fixture)
    {
        _userTestRepository = new UserTestRepository(fixture);
        _instituteTestRepository = new InstituteTestRepository(fixture);
    }

    #region Create

    [Fact]
    public async Task CreateAsync_ShouldPersistUser_WhenUserIsValid()
    {
        // Arrange
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var user = UserEntity
            .CreateAdmin(
            null,
            "test@test.com",
            "max_mustermann",
            "Max",
            "Mustermann",
            institute.Guid);

        await _userTestRepository.UserRepository
            .CreateAsync(user);
        await SaveChangesAsync();

        var persistedUser = await GetCreatedEntityByGuid<UserEntity>(user.Guid);

        Helpers.CheckCreatedUser(
            persistedUser,
            user,
            institute.Guid);
    }

    #endregion

    #region Get

    [Fact]
    public async Task GetUserByGuidAsync_ShouldReturnUser_WhenUserExists()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var creator = await _userTestRepository.CreateAdminAsync(
            institute.Guid);

        var user = await _userTestRepository
            .CreateTeacherAsync(
                institute.Guid,
                createdByUserGuid: creator.Guid);

        var persistedUser = await _userTestRepository
            .UserRepository
            .GetUserByGuidAsync(user.Guid, institute.Guid);

        Helpers.CheckCreatedUser(
            persistedUser,
            user,
            institute.Guid);

        persistedUser!.CreatedByUser.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserByGuidAsync_ShouldReturnNull_WhenUserIsNotInInstitute()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var creator = await _userTestRepository.CreateAdminAsync(
            institute.Guid);

        var user = await _userTestRepository
            .CreateTeacherAsync(
                institute.Guid,
                createdByUserGuid: creator.Guid);

        var persistedUser = await _userTestRepository
            .UserRepository
            .GetUserByGuidAsync(user.Guid, Guid.CreateVersion7());

        persistedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByGuidAsync_ShouldReturnNull_WhenUserNotExists()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        await _userTestRepository
            .CreateAdminAsync(
                institute.Guid);

        var persistedUser = await _userTestRepository
            .UserRepository
            .GetUserByGuidAsync(Guid.CreateVersion7(), Guid.CreateVersion7());

        persistedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnUser_WhenEmailMatches()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var user = await _userTestRepository
            .CreateAdminAsync(
            institute.Guid);

        var persistedUser = await _userTestRepository
            .UserRepository
            .GetUserByUsernameOrEmailWithRefreshTokenAsync(user.Email);

        Helpers.CheckCreatedUser(
            persistedUser,
            user,
            institute.Guid);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnUser_WhenUsernameMatches()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var user = await _userTestRepository
            .CreateAdminAsync(
                institute.Guid);

        var persistedUser = await _userTestRepository
            .UserRepository
            .GetUserByUsernameOrEmailWithRefreshTokenAsync(user.Username);

        Helpers.CheckCreatedUser(
            persistedUser,
            user,
            institute.Guid);
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnNull_WhenEmailNotFound()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        await _userTestRepository
            .CreateAdminAsync(
                institute.Guid);

        var persistedUser = await _userTestRepository
            .UserRepository
            .GetUserByUsernameOrEmailWithRefreshTokenAsync("test1@slais.de");

        persistedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnNull_WhenUsernameNotFound()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        await _userTestRepository
            .CreateAdminAsync(
                institute.Guid);

        var persistedUser = await _userTestRepository
            .UserRepository
            .GetUserByUsernameOrEmailWithRefreshTokenAsync("userTest1234");

        persistedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnAllUsers_WhenUserIsSuperAdmin()
    {
        var slaisInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var anotherInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var adminUser = await _userTestRepository.CreateAdminAsync(
            slaisInstitute.Guid);

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid);

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Test",
            lastName: "First");

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "Second");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Fest",
            lastName: "First");

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "First");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Johnny",
            lastName: "Flair");

        var users = await _userTestRepository.UserRepository.GetAllUsersFromInstituteAsync(
            anotherInstitute.Guid, Roles.SuperAdmin);

        Assert.NotNull(users);
        Assert.Equal(6, users.Count);

        users[0].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[0].Role.Should().Be(Roles.Admin);

        users[1].FirstName.Should().Be("Test");
        users[1].LastName.Should().Be("First");
        users[1].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[1].Role.Should().Be(Roles.Teacher);

        users[2].FirstName.Should().Be("Rest");
        users[2].LastName.Should().Be("Second");
        users[2].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[2].Role.Should().Be(Roles.Teacher);

        users[3].FirstName.Should().Be("Fest");
        users[3].LastName.Should().Be("First");
        users[3].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[3].Role.Should().Be(Roles.Student);

        users[4].FirstName.Should().Be("Rest");
        users[4].LastName.Should().Be("First");
        users[4].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[4].Role.Should().Be(Roles.Admin);

        users[5].FirstName.Should().Be("Johnny");
        users[5].LastName.Should().Be("Flair");
        users[5].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[5].Role.Should().Be(Roles.Student);

        Assert.All(users, user =>
        {
            Assert.Equal(anotherInstitute.Guid, user.InstituteGuid);
        });
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnAllTeacherAndStudents_WhenUserIsAdmin()
    {
        var slaisInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var anotherInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var adminUser = await _userTestRepository.CreateAdminAsync(
            slaisInstitute.Guid);

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid);

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Test",
            lastName: "First");

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "Second");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Fest",
            lastName: "First");

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "First");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Johnny",
            lastName: "Flair");

        var users = await _userTestRepository.UserRepository.GetAllUsersFromInstituteAsync(
            anotherInstitute.Guid, Roles.Admin);

        Assert.NotNull(users);
        Assert.Equal(4, users.Count);

        users[0].FirstName.Should().Be("Test");
        users[0].LastName.Should().Be("First");
        users[0].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[0].Role.Should().Be(Roles.Teacher);

        users[1].FirstName.Should().Be("Rest");
        users[1].LastName.Should().Be("Second");
        users[1].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[1].Role.Should().Be(Roles.Teacher);

        users[2].FirstName.Should().Be("Fest");
        users[2].LastName.Should().Be("First");
        users[2].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[2].Role.Should().Be(Roles.Student);

        users[3].FirstName.Should().Be("Johnny");
        users[3].LastName.Should().Be("Flair");
        users[3].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[3].Role.Should().Be(Roles.Student);

        Assert.All(users, user =>
        {
            Assert.Equal(anotherInstitute.Guid, user.InstituteGuid);
        });
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnStudents_WhenUserIsTeacher()
    {
        var slaisInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var anotherInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var adminUser = await _userTestRepository.CreateAdminAsync(
            slaisInstitute.Guid);

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid);

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Test",
            lastName: "First");

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "Second");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Fest",
            lastName: "First");

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "First");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Johnny",
            lastName: "Flair");

        var users = await _userTestRepository.UserRepository.GetAllUsersFromInstituteAsync(
            anotherInstitute.Guid, Roles.Teacher);

        Assert.NotNull(users);
        Assert.Equal(2, users.Count);

        users[0].FirstName.Should().Be("Fest");
        users[0].LastName.Should().Be("First");
        users[0].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[0].Role.Should().Be(Roles.Student);

        users[1].FirstName.Should().Be("Johnny");
        users[1].LastName.Should().Be("Flair");
        users[1].InstituteGuid.Should().Be(anotherInstitute.Guid);
        users[1].Role.Should().Be(Roles.Student);

        Assert.All(users, user =>
        {
            Assert.Equal(anotherInstitute.Guid, user.InstituteGuid);
        });
    }

    [Fact]
    public async Task GetAllUsers_ShouldNoReturnUsers_WhenUserIsStudent()
    {
        var slaisInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var anotherInstitute = await _instituteTestRepository.CreateInstituteAsync();

        var adminUser = await _userTestRepository.CreateAdminAsync(
            slaisInstitute.Guid);

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid);

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Test",
            lastName: "First");

        await _userTestRepository.CreateTeacherAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "Second");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Fest",
            lastName: "First");

        await _userTestRepository.CreateAdminAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Rest",
            lastName: "First");

        await _userTestRepository.CreateStudentAsync(
            anotherInstitute.Guid,
            createdByUserGuid: adminUser.Guid,
            firstName: "Johnny",
            lastName: "Flair");

        var users = await _userTestRepository.UserRepository.GetAllUsersFromInstituteAsync(
            anotherInstitute.Guid, Roles.Student);

        Assert.NotNull(users);
        Assert.Empty(users);
    }

    #endregion
}
