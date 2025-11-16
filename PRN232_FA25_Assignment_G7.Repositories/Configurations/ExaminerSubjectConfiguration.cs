using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories.Configurations;

public class ExaminerSubjectConfiguration : IEntityTypeConfiguration<ExaminerSubject>
{
    public void Configure(EntityTypeBuilder<ExaminerSubject> builder)
    {
        builder.ToTable("ExaminerSubjects");
        builder.HasKey(es => new { es.ExaminerId, es.SubjectId });

        builder.HasOne(es => es.Examiner)
               .WithMany(e => e.ExaminerSubjects)
               .HasForeignKey(es => es.ExaminerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.Subject)
               .WithMany(s => s.ExaminerSubjects)
               .HasForeignKey(es => es.SubjectId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
