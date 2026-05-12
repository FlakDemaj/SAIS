namespace Domain.Base;

public abstract class BaseGuidEntity
{
    public Guid Guid { get; init; }

    protected BaseGuidEntity()
    {
        Guid = Guid.CreateVersion7();
    }
}
