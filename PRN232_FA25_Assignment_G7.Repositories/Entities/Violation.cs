using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class Violation
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool IsZeroScore { get; set; }

    public Submission? Submission { get; set; }
}
