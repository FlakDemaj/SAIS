using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs;
using Application.Common.DTOs.Public.Users;
using Application.Interfaces;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Public.Users.Querys.GetUser;

public class GetUserQueryHandler :
    BaseHandler<GetUserQuery>,
    IRequestHandler<GetUserQuery, GetUserResponseDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(
        IUserRepository userRepository,
        IMapper mapper,
        ISlaisLogger<GetUserQuery> logger)
        : base(mapper, logger)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserResponseDto> HandleAsync(
        GetUserQuery request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository
            .GetUserByGuidAsync(request.Guid, authentication!.InstitutionGuid);

        if (user == null)
        {
            throw new SlaisException(UserErrorCodes.UserNotFound);
        }

        var requesterRole = authentication!.UserRole;

        if (!IsAllowedToView(requesterRole, user.Role))
        {
            throw new SlaisException(UserErrorCodes.Forbidden);
        }

        return _mapper.Map<GetUserResponseDto>(user);
    }

    private static bool IsAllowedToView(Roles callerRole, Roles targetRole)
    {
        switch (callerRole)
        {
            case Roles.Server:
            case Roles.SuperAdmin:
                return true;
            case Roles.Admin:
                return targetRole == Roles.Student || targetRole == Roles.Teacher;
            case Roles.Teacher:
                return targetRole == Roles.Student;
            default:
                return false;
        }
    }
}
