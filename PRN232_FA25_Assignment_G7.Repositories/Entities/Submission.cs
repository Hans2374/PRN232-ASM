using System;
using System.Collections.Generic;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public enum SubmissionStatus
{
    Pending = 0,
    Graded = 1,
    DoubleGraded = 2,
    Finalized = 3
}

public enum ReviewStatus
{
    None = 0,
    ModeratorPending = 1,
    AdminPending = 2,
    Completed = 3
}

public class Submission
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ExtractedFolderPath { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Workflow status
    public SubmissionStatus SubmissionStatus { get; set; } = SubmissionStatus.Pending;
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.None;
    
    // Grading information
    public string? GradingComments { get; set; }
    public Guid? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }
    
    // Double grading
    public Guid? SecondGradedBy { get; set; }
    public DateTime? SecondGradedAt { get; set; }
    public decimal? SecondScore { get; set; }
    public string? SecondGradingComments { get; set; }
    
    // Review comments
    public string? ModeratorComments { get; set; }
    public string? AdminComments { get; set; }

    // Import-related fields
    public Guid? ImportJobId { get; set; }
    public string? ExtractedText { get; set; }
    public string? FileHash { get; set; }
    public Guid? DuplicateGroupId { get; set; }

    public Exam? Exam { get; set; }
    public ImportJob? ImportJob { get; set; }
    public DuplicateGroup? DuplicateGroup { get; set; }
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public ICollection<SubmissionImage> Images { get; set; } = new List<SubmissionImage>();
}
