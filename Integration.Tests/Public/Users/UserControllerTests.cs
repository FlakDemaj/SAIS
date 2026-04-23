using System.Net;

using Application.Common.DTOs.Public.Users;
using Application.Public.Users;
using Application.Public.Users.Commands.CreateUser;

using Domain.Common.Enums;

using FluentAssertions;

using Integration.Tests.Common;

using Xunit;

namespace Integration.Tests.Public.Users;

public class UserControllerTests : TestBase
{
    public UserControllerTests(IntegrationContainerFixture fixture)
        : base(fixture)
    {
    }

    #region GetUsers

    [Fact]
    public async Task GetUsers_AsAdmin_ShouldReturnTeachersAndStudents()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.Role == Roles.Teacher || u.Role == Roles.Student);
    }

    [Fact]
    public async Task GetUsers_AsSuperAdmin_ShouldReturnAllUsers()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(Roles.SuperAdmin, Guid.CreateVersion7(), institute.Guid);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);

        result.Should().NotBeNull();
        result!.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetUsers_AsTeacher_ShouldReturnStudentsOnly()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(Roles.Teacher, teacher.Guid, institute.Guid);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);

        result.Should().NotBeNull();
        result!.Should().HaveCount(1);
        result[0].Role.Should().Be(Roles.Student);
    }

    [Fact]
    public async Task GetUsers_AsStudent_ShouldReturnForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();

        AuthenticateAs(Roles.Student, Guid.CreateVersion7(), institute.Guid);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_Unauthenticated_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_EmptyInstitute_ShouldReturnEmptyList()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);

        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserByGuid

    [Fact]
    public async Task GetUserByGuid_AsAdmin_ViewingTeacher_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{teacher.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<GetUserResponseDto>(response);

        result.Should().NotBeNull();
        result!.Email.Should().Be(teacher.Email);
        result.Role.Should().Be(Roles.Teacher);
    }

    [Fact]
    public async Task GetUserByGuid_AsAdmin_ViewingAdmin_ShouldReturnBadRequest()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var anotherAdmin = await _userRepo.CreateAdminAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{anotherAdmin.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserByGuid_NotFound_ShouldReturnBadRequest()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);

        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be((int)UserErrorCodes.UserNotFound);
    }

    [Fact]
    public async Task GetUserByGuid_Unauthenticated_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserByGuid_AsStudent_ShouldReturnForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();

        AuthenticateAs(Roles.Student, Guid.CreateVersion7(), institute.Guid);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(Roles.Teacher)]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Server)]
    [InlineData(Roles.SuperAdmin)]
    public async Task GetUserByGuid_AllowedRoles_ShouldNotReturnForbidden(Roles role)
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var student = await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(role, Guid.CreateVersion7(), institute.Guid);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{student.Guid}");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GetUserByPublicId (GuidResolver Pipeline)

    [Fact]
    public async Task GetUserByPublicId_AsAdmin_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{teacher.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<GetUserResponseDto>(response);

        result.Should().NotBeNull();
        result!.Email.Should().Be(teacher.Email);
    }

    #endregion

    #region CreateUser

    [Fact]
    public async Task CreateUser_AsAdmin_ShouldCreateTeacher()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var command = new CreateUserCommand
        {
            Email = "new.teacher@test.com",
            Firstname = "New",
            Lastname = "Teacher",
            Role = Roles.Teacher
        };

        var response = await _client.PostAsync(Routings.RestUserRouting, BuildJsonContent(command));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<CreateUserResponseDto>(response);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.UserGuid.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUser_AsAdmin_CreatingAdmin_ShouldReturnBadRequest()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(Roles.Admin, admin.Guid, institute.Guid);

        var command = new CreateUserCommand
        {
            Email = "another.admin@test.com",
            Firstname = "Another",
            Lastname = "Admin",
            Role = Roles.Admin
        };

        var response = await _client.PostAsync(Routings.RestUserRouting, BuildJsonContent(command));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_AsStudent_ShouldReturnForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();

        AuthenticateAs(Roles.Student, Guid.CreateVersion7(), institute.Guid);

        var command = new CreateUserCommand
        {
            Email = "student.create@test.com",
            Firstname = "Test",
            Lastname = "User",
            Role = Roles.Student
        };

        var response = await _client.PostAsync(Routings.RestUserRouting, BuildJsonContent(command));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
