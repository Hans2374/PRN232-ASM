using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .HasMaxLength(100);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
