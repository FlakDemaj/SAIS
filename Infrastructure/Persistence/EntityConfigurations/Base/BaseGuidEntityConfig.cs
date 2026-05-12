using Domain.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SLAIS.Domain.Commom;

namespace Infrastructure.Persistence.EntityConfigurations.Base;

public class BaseGuidEntityConfig<T> : IEntityTypeConfiguration<T>
    where T : BaseGuidEntity
{
    protected string _schema;
    protected string _prefix;

    public BaseGuidEntityConfig()
    {
        _schema = "public";
        _prefix = string.Empty;
    }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder
            .Property(p => p.Guid)
            .IsRequired()
            .HasColumnName(_prefix + "guid")
            .ValueGeneratedNever();

        builder.HasKey(p => p.Guid);
    }
}
