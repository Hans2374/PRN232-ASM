using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StudentCode).IsRequired().HasMaxLength(50);
        builder.Property(s => s.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(s => s.ExtractedFolderPath).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Score).HasPrecision(10, 2);
        builder.Property(s => s.SecondScore).HasPrecision(10, 2);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(s => s.GradingComments).HasMaxLength(2000);
        builder.Property(s => s.SecondGradingComments).HasMaxLength(2000);
        builder.Property(s => s.ModeratorComments).HasMaxLength(2000);
        builder.Property(s => s.AdminComments).HasMaxLength(2000);
        
        // Import-related properties
        builder.Property(s => s.ExtractedText).HasMaxLength(10000);
        builder.Property(s => s.FileHash).HasMaxLength(128);

        builder.HasMany(s => s.Violations)
               .WithOne(v => v.Submission)
               .HasForeignKey(v => v.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(s => s.Images)
               .WithOne(si => si.Submission)
               .HasForeignKey(si => si.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.ImportJob)
               .WithMany(ij => ij.Submissions)
               .HasForeignKey(s => s.ImportJobId)
               .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(s => s.DuplicateGroup)
               .WithMany(dg => dg.Submissions)
               .HasForeignKey(s => s.DuplicateGroupId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
