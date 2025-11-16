using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.ExamDate).IsRequired();

        builder.HasOne(e => e.Subject)
               .WithMany(s => s.Exams)
               .HasForeignKey(e => e.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Semester)
               .WithMany(s => s.Exams)
               .HasForeignKey(e => e.SemesterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Rubrics)
               .WithOne(r => r.Exam)
               .HasForeignKey(r => r.ExamId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Submissions)
               .WithOne(s => s.Exam)
               .HasForeignKey(s => s.ExamId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
