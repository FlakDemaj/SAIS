using Domain.Public.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.UserEntityConfig;

internal static class UserEntityForeignKeyExtension
{
    internal static void AddForeignKeys(this EntityTypeBuilder<UserEntity> builder)
    {
        builder
            .HasOne(u => u.CreatedByUser)
            .WithMany(u => u.CreatedUsers)
            .HasForeignKey(u => u.CreatedByUserGuid)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(u => u.UpdatedByUser)
            .WithMany(u => u.UpdatedUsers)
            .HasForeignKey(u => u.UpdatedByUserGuid)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(u => u.DeletedByUser)
            .WithMany(u => u.DeletedUsers)
            .HasForeignKey(u => u.DeletedByUserGuid)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(i => i.Institute)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.InstituteGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
