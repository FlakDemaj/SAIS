using Domain.Public.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.UserEntityConfig;

internal static class UserEntityIndexExtension
{
    internal static void AddIndexes(this EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasIndex(user => user.InstituteGuid)
            .HasDatabaseName("idx_users_institute_guid");

        builder
            .HasIndex(p => p.CreatedByUserGuid)
            .HasDatabaseName("idx_users_created_by");

        builder
            .HasIndex(p => p.UpdatedByUserGuid)
            .HasDatabaseName("idx_users_updated_by");

        builder
            .HasIndex(p => p.DeletedByUserGuid)
            .HasDatabaseName("idx_users_deleted_by");
    }
}
