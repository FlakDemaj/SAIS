using Application.Common.DTOs.Base;

using Domain.Common.Enums;

namespace Application.Common.DTOs.Public.Users;

public class GetUsersResponseDto : BaseIdDto
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public required Roles Role { get; set; }

    public States State { get; set; }

}
