using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Public.Users;
using Application.Common.Interfaces.Repositorys;
using Application.Interfaces;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

using SLAIS.Domain.Users;

namespace Application.Public.Users.Commands.CreateUser;

public class CreateUserCommandHandler :
    BaseHandler<CreateUserCommand>,
    IRequestHandler<CreateUserCommand, CreateUserResponseDto>
{

    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IMapper mapper,
        ISlaisLogger<CreateUserCommand> logger)
        : base(mapper, logger)
    {
        _userRepository = userRepository;
    }

    public async Task<CreateUserResponseDto> HandleAsync(
        CreateUserCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        CheckAuthorization(
            authentication!.UserRole,
            request.Role);

        var user = await CreateUser(
            request,
            authentication.UserGuid,
            authentication.InstitutionGuid);

        var createdUser = await _userRepository.CreateAsync(user);

        return new CreateUserResponseDto
        {
            Success = true,
            UserGuid = createdUser.Guid
        };
    }

    private async Task<UserEntity> CreateUser(
        CreateUserCommand request,
        Guid creatorGuid,
        Guid instituteGuid)
    {

        var username = await CreateUsername(
            request.Firstname,
            request.Lastname);

        switch (request.Role)
        {
            case Roles.Student:
                return UserEntity.CreateStudent(
                    creatorGuid,
                    request.Email,
                    username,
                    request.Firstname,
                    request.Lastname,
                    instituteGuid);
            case Roles.Teacher:
                return UserEntity.CreateTeacher(
                    creatorGuid,
                    request.Email,
                    username,
                    request.Firstname,
                    request.Lastname,
                    instituteGuid);
            case Roles.Admin:
                return UserEntity.CreateAdmin(
                    creatorGuid,
                    request.Email,
                    username,
                    request.Firstname,
                    request.Lastname,
                    instituteGuid);
            case Roles.SuperAdmin:
                return UserEntity.CreateSuperAdmin(
                    creatorGuid,
                    request.Email,
                    username,
                    request.Firstname,
                    request.Lastname,
                    instituteGuid);
            default:
                throw new SlaisException(UserErrorCodes.Forbidden);
        }
    }

    private async Task<string> CreateUsername(
        string firstname,
        string lastname)
    {
        var baseUsername = firstname + "." + lastname;
        var userNameWithTheSamePrefix = await
            _userRepository.GetUsernamesByPrefixAsync(baseUsername);

        if (!userNameWithTheSamePrefix.Contains(baseUsername))
        {
            return baseUsername;
        }

        var suffixUsername = userNameWithTheSamePrefix.Count;
        var username = baseUsername + suffixUsername;

        while (userNameWithTheSamePrefix.Contains(username))
        {
            suffixUsername++;
            username = baseUsername + suffixUsername;
        }

        return username;
    }

    private static void CheckAuthorization(
        Roles creatorRole,
        Roles roleToCreate)
    {
        if (creatorRole == Roles.SuperAdmin)
        {
            return;
        }

        if (creatorRole <= roleToCreate)
        {
            throw new SlaisException(UserErrorCodes.Forbidden);
        }
    }
}
