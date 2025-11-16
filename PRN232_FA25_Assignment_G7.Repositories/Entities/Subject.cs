using System;
using System.Collections.Generic;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class Subject
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<ExaminerSubject> ExaminerSubjects { get; set; } = new List<ExaminerSubject>();
}
