using Domain.System.RegistrationCodes;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.System.RegistrationCodeEntityConfig;

public static class RegistrationCodeEntityForeignKeyExtension
{
    internal static void AddForeignKeys(this EntityTypeBuilder<RegistrationCodeEntity> builder)
    {
        builder
            .HasOne(rc => rc.User)
            .WithMany(rc => rc.RegistrationCodes)
            .HasForeignKey(rc => rc.UserGuid);
    }
}
