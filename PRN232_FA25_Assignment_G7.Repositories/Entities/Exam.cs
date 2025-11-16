using System;
using System.Collections.Generic;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class Exam
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public Guid SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExamDate { get; set; }

    public Subject? Subject { get; set; }
    public Semester? Semester { get; set; }
    public ICollection<Rubric> Rubrics { get; set; } = new List<Rubric>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
