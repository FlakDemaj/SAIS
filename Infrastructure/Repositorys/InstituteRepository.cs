using Application.Interfaces;

using Domain.Institutes;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys.Base;

namespace Infrastructure.Repositorys;

public class InstituteRepository : BaseRepository<InstituteEntity>, IInstituteRepository
{
    public InstituteRepository(SlaisDbContext context)
        : base(context)
    {
    }
}
