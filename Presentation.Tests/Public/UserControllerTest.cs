using System.Net;

using Application.Common.Authentication;
using Application.Common.DTOs;
using Application.Common.DTOs.Base;
using Application.Common.DTOs.Public.Users;
using Application.Public.Users;
using Application.Public.Users.Querys.GetUser;
using Application.Public.Users.Querys.GetUsers;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using Presentation.Tests.Common;

using Xunit;

namespace Presentation.Tests.Public;

public class UserControllerTest : TestBase
{
    public UserControllerTest(TestWebApplicationFactory factory)
        : base(factory)
    {
    }

    #region GetUsers

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WhenRequestIsValid()
    {
        var expectedResult = new List<GetUsersResponseDto>
        {
            new()
            {
                Username = "john.doe",
                Email = "john@doe.de",
                Role = Roles.Admin,
                State = States.Deactive
            },
            new()
            {
                Username = "jane.doe",
                Email = "jane@doe.de",
                Role = Roles.Teacher,
                State = States.Active
            }
        };

        _factory.MediatorMock
            .SendAsync(
                Arg.Any<GetUsersQuery>(),
                Arg.Any<IAuthentication>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        AuthenticateAs(
            Roles.Admin,
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result[0].Username.Should().Be(expectedResult[0].Username);
        result[0].Email.Should().Be(expectedResult[0].Email);
        result[0].Role.Should().Be(expectedResult[0].Role);
        result[0].State.Should().Be(expectedResult[0].State);

        result[1].Username.Should().Be(expectedResult[1].Username);
        result[1].Email.Should().Be(expectedResult[1].Email);
        result[1].Role.Should().Be(expectedResult[1].Role);
        result[1].State.Should().Be(expectedResult[1].State);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUnauthorized_WhenUserRequestIsStudent()
    {
        var result = new List<GetUsersResponseDto>
        {
            new()
            {
                Username = "john.doe",
                Email = "john@doe.de",
                Role = Roles.Admin,
                State = States.Deleted
            },
            new()
            {
                Username = "jane.doe",
                Email = "jane@doe.de",
                Role = Roles.Teacher,
                State = States.Deactive
            }
        };

        _factory.MediatorMock
            .SendAsync(
                Arg.Any<GetUsersQuery>(),
                Arg.Any<IAuthentication>(),
                Arg.Any<CancellationToken>())
            .Returns(result);

        AuthenticateAs(
            Roles.Student,
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WhenUserListIsEmpty()
    {
        var expectedResult = new List<GetUsersResponseDto>();

        _factory.MediatorMock
            .SendAsync(
                Arg.Any<GetUsersQuery>(),
                Arg.Any<IAuthentication>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        AuthenticateAs(
            Roles.Admin,
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var response = await _client.GetAsync(Routings.RestUserRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<List<GetUsersResponseDto>>(response);

        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserById - OK

    [Fact]
    public async Task GetUserById_ShouldReturnOk_WhenRequestIsValid()
    {
        var publicId = 1;

        var expectedResult = new GetUserResponseDto
        {
            Username = "john.doe",
            Email = "john@doe.de",
            Role = Roles.Student,
            State = States.Active,
            BaseAuditCreated = new BaseAuditCreatedDto
            {
                CreationDate = new DateTime(2024, 1, 1),
                CreatedByFirstname = "admin",
                CreatedByLastname = "Test"
            },
            BaseAuditUpdated = new BaseAuditUpdatedDto
            {
                UpdatedDate = new DateTime(2024, 6, 1),
                UpdatedByFirstname = "admin",
                UpdatedByLastname = "Test"
            },
            BaseAuditDeleted = null
        };

        _factory.MediatorMock
            .SendAsync(
                Arg.Any<GetUserQuery>(),
                Arg.Any<IAuthentication>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        AuthenticateAs(
            Roles.Admin,
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{publicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<GetUserResponseDto>(response);

        result.Should().NotBeNull();
        result.Username.Should().Be(expectedResult.Username);
        result.Email.Should().Be(expectedResult.Email);
        result.Role.Should().Be(expectedResult.Role);
        result.State.Should().Be(expectedResult.State);

        result.BaseAuditCreated.Should().NotBeNull();
        result.BaseAuditCreated.CreationDate.Should().Be(expectedResult.BaseAuditCreated.CreationDate);
        result.BaseAuditCreated.CreatedByFirstname.Should().Be(expectedResult.BaseAuditCreated.CreatedByFirstname);
        result.BaseAuditCreated.CreatedByLastname.Should().Be(expectedResult.BaseAuditCreated.CreatedByLastname);

        result.BaseAuditUpdated.Should().NotBeNull();
        result.BaseAuditUpdated.UpdatedDate.Should().Be(expectedResult.BaseAuditUpdated.UpdatedDate);
        result.BaseAuditUpdated.UpdatedByFirstname.Should().Be(expectedResult.BaseAuditUpdated.UpdatedByFirstname);
        result.BaseAuditUpdated.UpdatedByLastname.Should().Be(expectedResult.BaseAuditUpdated.UpdatedByLastname);

        result.BaseAuditDeleted.Should().BeNull();
    }

    #endregion

    #region GetUserById - NotFound

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var publicId = 999;

        _factory.MediatorMock
            .SendAsync(
                Arg.Any<GetUserQuery>(),
                Arg.Any<IAuthentication>(),
                Arg.Any<CancellationToken>())
            .Throws(new SlaisException(UserErrorCodes.UserNotFound));

        AuthenticateAs(
            Roles.Admin,
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{publicId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetUserById - Unauthorized

    [Fact]
    public async Task GetUserById_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        var publicId = 1;

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{publicId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetUserById - Forbidden

    [Fact]
    public async Task GetUserById_ShouldReturnForbidden_WhenUserIsStudent()
    {
        var publicId = 1;

        AuthenticateAs(
            Roles.Student,
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{publicId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(Roles.Teacher)]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Server)]
    [InlineData(Roles.SuperAdmin)]
    public async Task GetUserById_ShouldReturnOk_WhenRoleIsAllowed(Roles role)
    {
        var publicId = 1;

        var expectedResult = new GetUserResponseDto
        {
            Username = "john.doe",
            Email = "john@doe.de",
            Role = Roles.Student,
            State = States.Active,
            BaseAuditCreated = new BaseAuditCreatedDto
            {
                CreationDate = new DateTime(2024, 1, 1),
                CreatedByFirstname = "admin",
                CreatedByLastname = "Test"
            },
            BaseAuditUpdated = null,
            BaseAuditDeleted = null
        };

        _factory.MediatorMock
            .SendAsync(
                Arg.Any<GetUserQuery>(),
                Arg.Any<IAuthentication>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        AuthenticateAs(
            role,
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var response = await _client.GetAsync($"{Routings.RestUserRouting}/{publicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
