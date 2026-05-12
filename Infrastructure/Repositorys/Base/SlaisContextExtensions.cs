using Application.Common;

using Domain.Base;
using Domain.Common.Exceptions;

using Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

using SLAIS.Domain.Commom;

namespace Infrastructure.Repositorys;

public static class SlaisContextExtensions
{
    public static async Task<TSaveType> CreateAsync<TSaveType>(
        this SlaisDbContext context,
        TSaveType saveData)
        where TSaveType : BaseGuidEntity
    {
        try
        {
            var result = context.Add(saveData);
            return result.Entity;
        }
        catch (SlaisException e)
        {
            throw new SlaisException(CommonErrorCodes.DatabaseError, e);
        }
    }

    public static IQueryable<T> GetNoTrackingSet<T>(this SlaisDbContext context)
        where T : class
    {
        return context.Set<T>().AsNoTracking();
    }

    public static IQueryable<T> GetTrackingSet<T>(
        this SlaisDbContext context)
        where T : class
    {
        return context.Set<T>();
    }
}
