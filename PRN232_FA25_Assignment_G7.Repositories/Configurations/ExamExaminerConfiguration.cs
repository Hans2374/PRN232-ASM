using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class ExamExaminerConfiguration : IEntityTypeConfiguration<ExamExaminer>
{
    public void Configure(EntityTypeBuilder<ExamExaminer> builder)
    {
        builder.ToTable("ExamExaminers");
        builder.HasKey(ee => new { ee.ExamId, ee.ExaminerId });

        builder.HasOne(ee => ee.Exam)
               .WithMany(e => e.ExamExaminers)
               .HasForeignKey(ee => ee.ExamId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ee => ee.Examiner)
               .WithMany(e => e.ExamExaminers)
               .HasForeignKey(ee => ee.ExaminerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ee => ee.AssignedAt)
               .IsRequired();

        builder.Property(ee => ee.IsPrimaryGrader)
               .IsRequired()
               .HasDefaultValue(true);
    }
}
