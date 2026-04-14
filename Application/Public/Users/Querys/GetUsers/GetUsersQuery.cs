using Application.Common.DTOs.Public.Users;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Public.Users.Querys.GetUsers;

public class GetUsersQuery : IRequest<List<GetUsersResponseDto>>, INoTransaction;
