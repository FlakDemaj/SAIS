using Application.Common.Mappers.Public;
using Application.Interfaces;
using Application.Public.Users.Querys.GetUsers;
using Application.Utils.Logger;

using AutoMapper;

using Domain.Common.Enums;

using FluentAssertions;

using NSubstitute;

using SLAIS.Domain.Users;

using Tests.Shared.TestDataCreator;

using Xunit;

namespace Application.Tests.Public.Users.Commands;

public class GetAllUsersCommandHandlerTests
{
    private readonly IUserRepository _userRepository;

    private readonly GetUsersQueryHandler _getUsersQueryHandler;

    public GetAllUsersCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();

        var logger = Substitute.For<ISlaisLogger<GetUsersQueryHandler>>();

        var mapper = new MapperConfiguration(config =>
        {
            config.AddProfile<UserMappingProfile>();
        }).CreateMapper();

        _getUsersQueryHandler = new GetUsersQueryHandler(
            logger,
            _userRepository,
            mapper);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnAllUsers()
    {
        var result = CreateUserTestData(
            2, 3, 4, 1);

        _userRepository.GetAllUsersFromInstituteAsync(
                Arg.Any<Guid>(), Arg.Any<Roles>())
            .Returns(result);

        var authentication = new Common.Authentication.Authentication(
            Guid.CreateVersion7(),
            Roles.Admin,
            Guid.CreateVersion7());

        var request = await _getUsersQueryHandler.HandleAsync(
            new GetUsersQuery(),
            authentication,
            CancellationToken.None);

        request.Should().NotBeNull();
        request.Count.Should().Be(result.Count);

        for (var i = 0; i < result.Count; i++)
        {
            request[i].Email.Should().Be(result[i].Email);
            request[i].Id.Should().Be(result[i].Id);
            request[i].Role.Should().Be(result[i].Role);
            request[i].Username.Should().Be(result[i].Username);
            request[i].State.Should().Be(result[i].State);
        }
    }

    private static List<UserEntity> CreateUserTestData(
        short numberOfStudents,
        short numberOfTeachers,
        short numberOfAdmins,
        short numberOfSuperAdmins)
    {
        var users = new List<UserEntity>();

        var adminUserCreation = UserTestData.CreateUser();

        for (var i = 0; i < numberOfStudents; i++)
        {
            users.Add(UserTestData.CreateUser(
                roles: Roles.Student,
                createdByUser: adminUserCreation));
        }

        for (var i = 0; i < numberOfTeachers; i++)
        {
            users.Add(UserTestData.CreateUser(
                roles: Roles.Admin,
                createdByUser: adminUserCreation));
        }

        for (var i = 0; i < numberOfAdmins; i++)
        {
            users.Add(UserTestData.CreateUser(
                roles: Roles.Teacher,
                createdByUser: adminUserCreation));
        }

        for (var i = 0; i < numberOfSuperAdmins; i++)
        {
            users.Add(UserTestData.CreateUser(
                roles: Roles.SuperAdmin,
                createdByUser: adminUserCreation));
        }

        return users;
    }
}
