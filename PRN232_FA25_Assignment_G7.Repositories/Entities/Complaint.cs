using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public enum ComplaintStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Escalated = 3
}

public class Complaint
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EvidencePath { get; set; } // Path to uploaded evidence file
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Submission? Submission { get; set; }
    public User? Reviewer { get; set; }
}