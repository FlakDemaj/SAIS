using Application.Common.Interfaces.Repositorys;
using Application.Interfaces;

using Infrastructure.Persistence.Context;

using SLAIS.Domain.Commom;

namespace Infrastructure.Repositorys;

public abstract class BaseRepository<T> : SlaisRepository, IBaseRepository<T>
    where T : BaseGuidEntity
{
    protected BaseRepository(SlaisDbContext context)
        : base(context)
    {
    }

    public Task<T> CreateAsync(T objectToCreate)
    {
        return _context.CreateAsync(objectToCreate);
    }
}
