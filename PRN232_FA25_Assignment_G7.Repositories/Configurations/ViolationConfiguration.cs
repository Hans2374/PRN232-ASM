using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class ViolationConfiguration : IEntityTypeConfiguration<Violation>
{
    public void Configure(EntityTypeBuilder<Violation> builder)
    {
        builder.ToTable("Violations");
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Type).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Description).IsRequired().HasMaxLength(1000);
        builder.Property(v => v.Severity).IsRequired();
        builder.Property(v => v.IsZeroScore).IsRequired().HasDefaultValue(false);
        builder.Property(v => v.ReviewComments).HasMaxLength(2000);
        builder.Property(v => v.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(v => v.Submission)
               .WithMany(s => s.Violations)
               .HasForeignKey(v => v.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
