using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class RubricConfiguration : IEntityTypeConfiguration<Rubric>
{
    public void Configure(EntityTypeBuilder<Rubric> builder)
    {
        builder.ToTable("Rubrics");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Criteria).IsRequired().HasMaxLength(500);
        builder.Property(r => r.MaxScore).IsRequired();
    }
}
