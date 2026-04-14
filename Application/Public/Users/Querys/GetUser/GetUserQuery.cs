using Application.Common.DTOs.Public.Users;
using Application.Common.Interfaces;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Public.Users.Querys.GetUser;

public class GetUserQuery : IRequest<GetUserResponseDto>, IHasGuid, INoTransaction
{
    public int PublicId { get; init; }

    public Guid Guid { get; set; }

    public string EntityType
    {
        get { return "User"; }
    }
}
