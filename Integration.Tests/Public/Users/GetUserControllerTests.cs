using System.Net;

using Application.Common.DTOs.Public.Users;
using Application.Public.Users;

using Domain.Common.Enums;

using FluentAssertions;

using Integration.Tests.Common;

using Xunit;

namespace Integration.Tests.Public.Users;

public class GetUserControllerTests : TestBase
{
    public GetUserControllerTests(IntegrationContainerFixture fixture) : base(fixture)
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
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
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
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
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
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
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
        error!.ErrorCode.Should().Be((int)UserErrorCodes.Forbidden);
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
        error!.ErrorCode.Should().Be((int)UserErrorCodes.UserNotFound);
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
        error!.ErrorCode.Should().Be((int)UserErrorCodes.UserNotFound);
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
}
