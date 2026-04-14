using Application.Common.DTOs.Public.Users;
using Application.Utils.Interfaces.Mediator;

using Domain.Common.Enums;

namespace Application.Public.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<CreateUserResponseDto>
{
    public required string Email { get; init; }

    public required string Firstname { get; init; }

    public string Lastname { get; init; }

    public Roles Role { get; init; }
}
