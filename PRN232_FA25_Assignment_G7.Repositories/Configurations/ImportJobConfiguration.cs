using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.ToTable("ImportJobs");
        builder.HasKey(ij => ij.Id);
        
        builder.Property(ij => ij.ArchiveName).IsRequired().HasMaxLength(500);
        builder.Property(ij => ij.SubjectCode).IsRequired().HasMaxLength(50);
        builder.Property(ij => ij.SemesterCode).IsRequired().HasMaxLength(50);
        builder.Property(ij => ij.UploaderUserId).IsRequired();
        builder.Property(ij => ij.Status).IsRequired().HasDefaultValue(ImportJobStatus.Pending);
        builder.Property(ij => ij.TotalFiles).IsRequired().HasDefaultValue(0);
        builder.Property(ij => ij.ProcessedFiles).IsRequired().HasDefaultValue(0);
        builder.Property(ij => ij.SuccessCount).IsRequired().HasDefaultValue(0);
        builder.Property(ij => ij.FailedCount).IsRequired().HasDefaultValue(0);
        builder.Property(ij => ij.ViolationsCount).IsRequired().HasDefaultValue(0);
        builder.Property(ij => ij.StartedAt).IsRequired();
        builder.Property(ij => ij.StorageFolderPath).IsRequired().HasMaxLength(1000);
        builder.Property(ij => ij.ErrorMessage).HasMaxLength(2000);
        
        builder.HasOne(ij => ij.Uploader)
               .WithMany()
               .HasForeignKey(ij => ij.UploaderUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}