using System;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public enum ImportJobStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

public class ImportJob
{
    public Guid Id { get; set; }
    public string ArchiveName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string SemesterCode { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public Guid UploaderUserId { get; set; }
    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int ViolationsCount { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string StorageFolderPath { get; set; } = string.Empty;

    public User? Uploader { get; set; }
    public Exam? Exam { get; set; }
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}