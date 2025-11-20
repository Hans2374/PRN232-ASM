using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("Complaints");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.StudentCode).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.EvidencePath).HasMaxLength(500);
        builder.Property(c => c.Status).IsRequired().HasDefaultValue(ComplaintStatus.Pending);
        builder.Property(c => c.ReviewComments).HasMaxLength(2000);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(c => c.Submission)
               .WithMany(s => s.Complaints)
               .HasForeignKey(c => c.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Reviewer)
               .WithMany()
               .HasForeignKey(c => c.ReviewedBy)
               .OnDelete(DeleteBehavior.SetNull);
    }
}