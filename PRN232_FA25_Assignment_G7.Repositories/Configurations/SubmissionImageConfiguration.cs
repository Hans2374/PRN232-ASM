using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class SubmissionImageConfiguration : IEntityTypeConfiguration<SubmissionImage>
{
    public void Configure(EntityTypeBuilder<SubmissionImage> builder)
    {
        builder.ToTable("SubmissionImages");
        builder.HasKey(si => si.Id);
        
        builder.Property(si => si.SubmissionId).IsRequired();
        builder.Property(si => si.ImagePath).IsRequired().HasMaxLength(1000);
        builder.Property(si => si.OriginalFileName).IsRequired().HasMaxLength(500);
        builder.Property(si => si.FileSize).IsRequired();
        builder.Property(si => si.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(si => si.Submission)
               .WithMany(s => s.Images)
               .HasForeignKey(si => si.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}