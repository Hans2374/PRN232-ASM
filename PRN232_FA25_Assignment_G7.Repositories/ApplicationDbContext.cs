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
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Seed admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "System Administrator",
                Email = "admin@example.com",
                Role = Role.Admin,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );
    }
}
