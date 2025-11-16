using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class ExaminerSubject
{
    public Guid ExaminerId { get; set; }
    public Guid SubjectId { get; set; }

    public Examiner? Examiner { get; set; }
    public Subject? Subject { get; set; }
}
