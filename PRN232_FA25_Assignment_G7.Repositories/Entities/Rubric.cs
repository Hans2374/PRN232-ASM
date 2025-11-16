using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class Rubric
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string Criteria { get; set; } = string.Empty;
    public int MaxScore { get; set; }

    public Exam? Exam { get; set; }
}
