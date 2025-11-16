using System;
using System.Collections.Generic;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class Semester
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
