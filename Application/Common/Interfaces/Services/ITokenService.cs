using Application.Authentication.Commands.Login;

using Domain.Common.Enums;

namespace Application.Common.Interfaces.Services;

public interface ITokenService
{
    GeneratedAccessTokenResult GenerateAccessToken(Guid userGuid,
        Roles userRole,
        Guid instituteGuid);
}
