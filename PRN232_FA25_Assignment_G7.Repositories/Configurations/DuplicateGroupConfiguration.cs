using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class DuplicateGroupConfiguration : IEntityTypeConfiguration<DuplicateGroup>
{
    public void Configure(EntityTypeBuilder<DuplicateGroup> builder)
    {
        builder.ToTable("DuplicateGroups");
        builder.HasKey(dg => dg.Id);
        
        builder.Property(dg => dg.ExamId).IsRequired();
        builder.Property(dg => dg.GroupName).IsRequired().HasMaxLength(200);
        builder.Property(dg => dg.SimilarityScore).HasPrecision(5, 4);
        builder.Property(dg => dg.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(dg => dg.Exam)
               .WithMany()
               .HasForeignKey(dg => dg.ExamId)
               .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(dg => dg.Submissions)
               .WithOne(s => s.DuplicateGroup)
               .HasForeignKey(s => s.DuplicateGroupId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}