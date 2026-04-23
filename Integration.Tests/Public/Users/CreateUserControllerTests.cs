using System.Net;
using System.Net.Http.Json;

using Application.Common.DTOs.Public.Users;
using Application.Public.Users;
using Application.Public.Users.Commands.CreateUser;

using Domain.Common.Enums;

using FluentAssertions;

using Integration.Tests.Common;

using Xunit;

namespace Integration.Tests.Public.Users;

public class CreateUserControllerTests : TestBase
{
    public CreateUserControllerTests(IntegrationContainerFixture fixture)
        : base(fixture)
    {
    }

    #region CreateUser – success

    [Fact]
    public async Task CreateUser_AsAdmin_CreatingTeacher_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(admin);

        var command = new CreateUserCommand
        {
            Email = "new.teacher@test.com",
            Firstname = "New",
            Lastname = "Teacher",
            Role = Roles.Teacher
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<CreateUserResponseDto>(response);
        result!.Success.Should().BeTrue();
        result.UserGuid.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUser_AsAdmin_CreatingStudent_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(admin);

        var command = new CreateUserCommand
        {
            Email = "new.student@test.com",
            Firstname = "New",
            Lastname = "Student",
            Role = Roles.Student
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_AsTeacher_CreatingStudent_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(teacher);

        var command = new CreateUserCommand
        {
            Email = "teacher.student@test.com",
            Firstname = "Teacher",
            Lastname = "Student",
            Role = Roles.Student
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_AsSuperAdmin_CreatingAdmin_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var superAdmin = await _userRepo.CreateSuperAdminAsync(institute.Guid);

        AuthenticateAs(superAdmin);

        var command = new CreateUserCommand
        {
            Email = "superadmin.admin@test.com",
            Firstname = "Super",
            Lastname = "Admin",
            Role = Roles.Admin
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_AsSuperAdmin_CreatingSuperAdmin_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var superAdmin = await _userRepo.CreateSuperAdminAsync(institute.Guid);

        AuthenticateAs(superAdmin);

        var command = new CreateUserCommand
        {
            Email = "superadmin2@test.com",
            Firstname = "Another",
            Lastname = "SuperAdmin",
            Role = Roles.SuperAdmin
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region CreateUser – authorization checks (Forbidden)

    [Fact]
    public async Task CreateUser_AsAdmin_CreatingAdmin_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(admin);

        var command = new CreateUserCommand
        {
            Email = "another.admin@test.com",
            Firstname = "Another",
            Lastname = "Admin",
            Role = Roles.Admin
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
    }

    [Fact]
    public async Task CreateUser_AsAdmin_CreatingSuperAdmin_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(admin);

        var command = new CreateUserCommand
        {
            Email = "new.superadmin@test.com",
            Firstname = "New",
            Lastname = "SuperAdmin",
            Role = Roles.SuperAdmin
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
    }

    [Fact]
    public async Task CreateUser_AsTeacher_CreatingTeacher_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(teacher);

        var command = new CreateUserCommand
        {
            Email = "another.teacher@test.com",
            Firstname = "Another",
            Lastname = "Teacher",
            Role = Roles.Teacher
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
    }

    [Fact]
    public async Task CreateUser_AsTeacher_CreatingAdmin_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(teacher);

        var command = new CreateUserCommand
        {
            Email = "teacher.admin@test.com",
            Firstname = "Teacher",
            Lastname = "Admin",
            Role = Roles.Admin
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
    }

    #endregion

    #region CreateUser – access control

    [Fact]
    public async Task CreateUser_AsStudent_ShouldReturnForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var command = new CreateUserCommand
        {
            Email = "student.create@test.com",
            Firstname = "Test",
            Lastname = "User",
            Role = Roles.Student
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_Unauthenticated_ShouldReturnUnauthorized()
    {
        var command = new CreateUserCommand
        {
            Email = "unauth@test.com",
            Firstname = "Test",
            Lastname = "User",
            Role = Roles.Student
        };

        var response = await _client.PostAsJsonAsync(Routings.RestUserRouting, command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
