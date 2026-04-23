using Infrastructure.Persistence.EntityConfigurations.Base;
using Infrastructure.Persistence.EntityConfigurations.Entitys.UserEntityConfig;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SLAIS.Domain.Users;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys;

internal sealed class UserEntityAttributeConfig : BaseIdEntityConfig<UserEntity>
{
    private string Table { get; }

    public UserEntityAttributeConfig()
    {
        Table = "users";
        _schema = "public";
        _prefix = "user_";
    }

    public override void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable(Table, _schema);

        base.Configure(builder);

        builder
            .Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired();

        builder
            .Property(u => u.HashedPassword)
            .HasColumnName("password_hashed");

        builder
            .Property(u => u.Username)
            .HasColumnName("username")
            .IsRequired();

        builder
            .Property(u => u.FirstName)
            .HasColumnName("first_name")
            .IsRequired();

        builder
            .Property(u => u.LastName)
            .HasColumnName("last_name")
            .IsRequired();

        builder
            .Property(u => u.Role)
            .HasColumnName("role_id")
            .IsRequired();

        builder
            .Property(u => u.LoginAttempts)
            .HasColumnName("login_attempts")
            .IsRequired()
            .HasDefaultValue(0);

        builder
            .Property(u => u.IsBlocked)
            .HasColumnName("is_blocked")
            .IsRequired()
            .HasDefaultValue(false);

        builder
            .Property(u => u.InstituteGuid)
            .HasColumnName("fk_institute_guid")
            .IsRequired();

        builder
            .Property(u => u.State)
            .HasColumnName("state")
            .IsRequired();

        builder.AddForeignKeys();
        builder.AddIndexes();
    }
}
