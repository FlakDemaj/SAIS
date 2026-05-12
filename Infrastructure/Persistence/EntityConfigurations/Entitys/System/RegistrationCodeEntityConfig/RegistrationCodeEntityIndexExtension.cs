using Domain.System.RegistrationCodes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.System.RegistrationCodeEntityConfig;

public static class RegistrationCodeEntityIndexExtension
{
    internal static void AddIndexes(this EntityTypeBuilder<RegistrationCodeEntity> builder)
    {
        builder
            .HasIndex(rt => rt.UserGuid)
            .HasDatabaseName("idx_registration_code_user");
    }
}
