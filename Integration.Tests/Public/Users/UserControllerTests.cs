using System.Net;
using System.Net.Http.Json;

using Application.Common.DTOs.Public.Users;
using Application.Public.Users.Commands.CreateUser;

using Domain.Common.Enums;

using FluentAssertions;

using Integration.Tests.Common;

using Xunit;

using AppUserErrors = Application.Public.Users.UserErrorCodes;

namespace Integration.Tests.Public.Users;

public class UserControllerTests : TestBase
{
    public UserControllerTests(IntegrationContainerFixture fixture)
        : base(fixture)
    {
    }

    #region GetUsers – role-based filtering

    [Fact]
    public async Task GetUsers_AsAdmin_ShouldReturnTeachersAndStudentsOnly()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(admin);

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
        var superAdmin = await _userRepo.CreateSuperAdminAsync(institute.Guid);
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(superAdmin);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);
        result!.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetUsers_AsTeacher_ShouldReturnStudentsOnly()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(teacher);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);
        result!.Should().HaveCount(1);
        result[0].Role.Should().Be(Roles.Student);
    }

    [Fact]
    public async Task GetUsers_EmptyInstitute_ShouldReturnEmptyList()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUsers – access control

    [Fact]
    public async Task GetUsers_AsStudent_ShouldReturnForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_Unauthenticated_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetUserByGuid – success

    [Fact]
    public async Task GetUserByGuid_AsAdmin_ViewingTeacher_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{teacher.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<GetUserResponseDto>(response);
        result!.Email.Should().Be(teacher.Email);
        result.Role.Should().Be(Roles.Teacher);
    }

    [Fact]
    public async Task GetUserByGuid_AsAdmin_ViewingStudent_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var student = await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{student.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserByGuid_AsTeacher_ViewingStudent_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        var student = await _userRepo.CreateStudentAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(teacher);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{student.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserByGuid_AsSuperAdmin_ViewingAdmin_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var superAdmin = await _userRepo.CreateSuperAdminAsync(institute.Guid);
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(superAdmin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{admin.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GetUserByGuid – access-level checks (Forbidden)

    [Fact]
    public async Task GetUserByGuid_AsAdmin_ViewingAdmin_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var anotherAdmin = await _userRepo.CreateAdminAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{anotherAdmin.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
    }

    [Fact]
    public async Task GetUserByGuid_AsAdmin_ViewingSuperAdmin_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var superAdmin = await _userRepo.CreateSuperAdminAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{superAdmin.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
    }

    [Fact]
    public async Task GetUserByGuid_AsTeacher_ViewingTeacher_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);
        var anotherTeacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(teacher);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{anotherTeacher.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
    }

    [Fact]
    public async Task GetUserByGuid_AsTeacher_ViewingAdmin_ShouldReturnBadRequest_WithForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(teacher);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{admin.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
    }

    #endregion

    #region GetUserByGuid – not found

    [Fact]
    public async Task GetUserByGuid_WhenUserNotFound_ShouldReturnBadRequest_WithUserNotFound()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AppUserErrors.UserNotFound);
    }

    [Fact]
    public async Task GetUserByGuid_WhenUserIsInDifferentInstitute_ShouldReturnBadRequest_WithUserNotFound()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var otherInstitute = await _instituteRepo.CreateInstituteAsync();

        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(otherInstitute.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{teacher.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AppUserErrors.UserNotFound);
    }

    #endregion

    #region GetUserByGuid – access control

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
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GetUserByPublicId (GuidResolver pipeline)

    [Fact]
    public async Task GetUserByPublicId_AsAdmin_ShouldReturnOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        var teacher = await _userRepo.CreateTeacherAsync(institute.Guid, createdByUserGuid: admin.Guid);

        AuthenticateAs(admin);

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{teacher.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<GetUserResponseDto>(response);
        result!.Email.Should().Be(teacher.Email);
    }

    #endregion

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
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
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
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
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
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
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
        error!.ErrorCode.Should().Be((int)AppUserErrors.Forbidden);
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
