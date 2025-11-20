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
        
        builder.Property(v => v.ViolationType).IsRequired();
        builder.Property(v => v.Severity).IsRequired().HasDefaultValue(ViolationSeverity.Medium);
        builder.Property(v => v.Description).IsRequired().HasMaxLength(1000);
        builder.Property(v => v.Evidence).HasConversion(
            v => string.Join(";", v),
            v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
        );
        builder.Property(v => v.ConfidenceScore).HasPrecision(5, 4);
        builder.Property(v => v.CreatedBy).IsRequired();
        builder.Property(v => v.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(v => v.Status).IsRequired().HasDefaultValue(ViolationStatus.New);
        
         builder.Property<Guid?>(nameof(Violation.SubmissionId)).IsRequired(false);
         builder.HasOne(v => v.Submission)
             .WithMany(s => s.Violations)
             .HasForeignKey(v => v.SubmissionId)
             .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(v => v.Creator)
               .WithMany()
               .HasForeignKey(v => v.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
