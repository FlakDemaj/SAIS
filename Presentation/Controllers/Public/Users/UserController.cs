using Application.Common.DTOs.Public.Users;
using Application.Public.Users.Commands.CreateUser;
using Application.Public.Users.Querys.GetUser;
using Application.Public.Users.Querys.GetUsers;
using Application.Utils.Interfaces.Mediator;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Presentation.Utils;

namespace Presentation.Controllers.Public.Users;

[Authorize(Roles = $"{Role.Teacher},{Role.Admin},{Role.Server},{Role.SuperAdmin}")]
public class UserController : BaseRestController
{
    public UserController(
        IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    public async Task<ActionResult<List<GetUsersResponseDto>>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetUsersQuery(),
            Authentication,
            cancellationToken);
    }

    [HttpGet("{publicId:int}")]
    public async Task<ActionResult<GetUserResponseDto>> GetUserByIdAsync(
        [FromRoute] int publicId,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetUserQuery { PublicId = publicId },
            Authentication,
            cancellationToken);
    }

    [HttpGet("{userGuid:guid}")]
    public async Task<ActionResult<GetUserResponseDto>> GetUserByGuidAsync(
        [FromRoute] Guid userGuid,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetUserQuery { Guid = userGuid },
            Authentication,
            cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserResponseDto>> CreateUserAsync(
        [FromBody] CreateUserCommand createUserCommand,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
           createUserCommand,
           Authentication,
           cancellationToken);
    }
}
