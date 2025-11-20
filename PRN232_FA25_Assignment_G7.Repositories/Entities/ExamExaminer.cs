using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class ExamExaminer
{
    public Guid ExamId { get; set; }
    public Guid ExaminerId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsPrimaryGrader { get; set; } = true;

    public Exam? Exam { get; set; }
    public Examiner? Examiner { get; set; }
}
