using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Repositories;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Rubric> Rubrics => Set<Rubric>();
    public DbSet<Examiner> Examiners => Set<Examiner>();
    public DbSet<ExaminerSubject> ExaminerSubjects => Set<ExaminerSubject>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Violation> Violations => Set<Violation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
