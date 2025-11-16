using System;
using System.Collections.Generic;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ExtractedFolderPath { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }

    public Exam? Exam { get; set; }
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
