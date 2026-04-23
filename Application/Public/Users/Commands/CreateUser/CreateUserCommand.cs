using Application.Common.DTOs.Base;
using Application.Common.DTOs.Public.Users;
using Application.Utils.Interfaces.Mediator;

using Domain.Common.Enums;

namespace Application.Public.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<CreateObjectResponseDto>
{
    public required string Email { get; init; }

    public required string Firstname { get; init; }

    public required string Lastname { get; init; }

    public Roles Role { get; init; }
}
