using SLAIS.Domain.Commom;

namespace Application.Common.Interfaces.Repositorys;

public interface IBaseRepository<T>
    where T : BaseGuidEntity
{
    Task<T> CreateAsync(T objectToCreate);
}
