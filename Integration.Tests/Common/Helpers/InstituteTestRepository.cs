using Domain.Institutes;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys;

namespace Integration.Tests.Common.Helpers;

public class InstituteTestRepository
{
    public readonly InstituteRepository InstituteRepository;

    private readonly SlaisDbContext _dbContext;

    public InstituteTestRepository(SlaisDbContext dbContext)
    {
        _dbContext = dbContext;
        InstituteRepository = new InstituteRepository(dbContext);
    }

    public async Task<InstituteEntity> CreateInstituteAsync(
        Guid? createdByUserGuid = null,
        string? name = null,
        string? branch = null)
    {
        var institute = InstituteEntity.Create(
            createdByUserGuid,
            name ?? "TestInstitute",
            branch ?? "Health");

        institute = await InstituteRepository.CreateAsync(institute);
        await _dbContext.SaveChangesAsync();

        return institute;
    }
}
