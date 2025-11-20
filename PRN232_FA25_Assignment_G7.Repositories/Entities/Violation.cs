using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public enum ViolationReviewStatus
{
    Pending = 0,
    ModeratorApproved = 1,
    ModeratorRejected = 2,
    AdminApproved = 3,
    AdminRejected = 4,
    Escalated = 5
}

public class Violation
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool IsZeroScore { get; set; }
    
    // Review workflow
    public ViolationReviewStatus ReviewStatus { get; set; } = ViolationReviewStatus.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Submission? Submission { get; set; }
}
