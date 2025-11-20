using System;
using System.Collections.Generic;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public enum ViolationType
{
    FilenameViolation = 0,
    Duplicate = 1,
    Plagiarism = 2,
    InvalidFormat = 3
}

public enum ViolationSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum ViolationStatus
{
    New = 0,
    Reviewed = 1,
    Escalated = 2,
    Resolved = 3
}

    public class Violation
    {
        public Guid Id { get; set; }
        // SubmissionId is optional: some violations (e.g. filename format) may not map to a saved submission
        public Guid? SubmissionId { get; set; }
    public ViolationType ViolationType { get; set; }
    public ViolationSeverity Severity { get; set; } = ViolationSeverity.Medium;
    public string Description { get; set; } = string.Empty;
    public List<string> Evidence { get; set; } = new List<string>();
    public decimal ConfidenceScore { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public ViolationStatus Status { get; set; } = ViolationStatus.New;

    public Submission? Submission { get; set; }
    public User? Creator { get; set; }
}
