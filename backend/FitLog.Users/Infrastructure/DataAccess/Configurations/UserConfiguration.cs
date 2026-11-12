using FitLog.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLog.Users.Infrastructure.DataAccess.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // Id приходит из токена (claim sub), база его не генерирует.
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(User.EmailMaxLength);

        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
