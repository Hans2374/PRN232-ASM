using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class SubmissionImage
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }

    public Submission? Submission { get; set; }
}