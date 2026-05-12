using Application.Common;

using Domain.Common.Exceptions;
using Domain.Public.Users;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Pipelines.Guid;

public class GuidResolver
{
    private readonly SlaisDbContext _dbContext;

    public GuidResolver(SlaisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<System.Guid> ResolveAsync(
        int publicId,
        string entityType,
        CancellationToken cancellationToken)
    {
        var guidId = entityType switch
        {
            "User" => await _dbContext.GetNoTrackingSet<UserEntity>()
                .Where(p => p.Id == publicId)
                .Select(p => (System.Guid?)p.Guid)
                .FirstOrDefaultAsync(cancellationToken),

            _ => throw new SlaisException(CommonErrorCodes.DefaultErrorCode)
        };

        if (guidId is null)
        {
            throw new SlaisException(CommonErrorCodes.DefaultErrorCode);
        }

        return guidId.Value;
    }
}
