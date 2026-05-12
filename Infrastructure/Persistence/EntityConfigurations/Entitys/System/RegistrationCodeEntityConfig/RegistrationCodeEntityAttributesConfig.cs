using Domain.System.RegistrationCodes;

using Infrastructure.Persistence.EntityConfigurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.System.RegistrationCodeEntityConfig;

public class RegistrationCodeEntityAttributesConfig : BaseGuidEntityConfig<RegistrationCodeEntity>
{
    private string Table { get; }

    public RegistrationCodeEntityAttributesConfig()
    {
        Table = "registration_codes";
        _schema = "system";
        _prefix = "refresh_token_";
    }

    public override void Configure(EntityTypeBuilder<RegistrationCodeEntity> builder)
    {
        builder.ToTable(Table, _schema);

        base.Configure(builder);

        builder
            .Property(rc => rc.RegistrationCode)
            .HasColumnName("registration_code")
            .IsRequired();

        builder
            .Property(rc => rc.Revoked)
            .HasColumnName("revoked")
            .IsRequired();

        builder
            .Property(rc => rc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.AddForeignKeys();
        builder.AddIndexes();
    }
}
