using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Public.Users;
using Application.Interfaces;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

namespace Application.Public.Users.Querys.GetUsers;

public class GetUsersQueryHandler :
    BaseHandler<GetUsersQueryHandler>,
    IRequestHandler<GetUsersQuery, List<GetUsersResponseDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(
        ISlaisLogger<GetUsersQueryHandler> logger,
        IUserRepository userRepository,
        IMapper mapper)
        : base(mapper, logger)
    {
        _userRepository = userRepository;
    }

    public async Task<List<GetUsersResponseDto>> HandleAsync(
        GetUsersQuery request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllUsersFromInstituteAsync(
            authentication!.InstitutionGuid, authentication.UserRole);

        return _mapper.Map<List<GetUsersResponseDto>>(users);

    }
}
