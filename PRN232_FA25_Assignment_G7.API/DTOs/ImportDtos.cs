using System.ComponentModel.DataAnnotations;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.DTOs
{
    public class ImportRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public Guid SubjectId { get; set; }

        [Required]
        public Guid SemesterId { get; set; }

        [Required]
        public Guid ExamId { get; set; }
    }

    public class ImportStatusResponse
    {
        public Guid JobId { get; set; }
        public ImportJobStatus Status { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int ViolationsCount { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> ProgressMessages { get; set; } = new();
    }

    public class ImportResultsResponse
    {
        public Guid JobId { get; set; }
        public ImportJobStatus Status { get; set; }
        public List<ImportResultItemResponse> SuccessfulImports { get; set; } = new();
        public List<ImportFailureResponse> FailedImports { get; set; } = new();
        public List<DuplicateGroupResponse> DuplicateGroups { get; set; } = new();
        public List<ViolationResponse> Violations { get; set; } = new();
        public ImportSummaryResponse Summary { get; set; } = null!;
    }

    public class ImportResultItemResponse
    {
        public Guid SubmissionId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public DateTime ImportedAt { get; set; }
    }

    public class ImportFailureResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
    }

    public class DuplicateGroupResponse
    {
        public Guid GroupId { get; set; }
        public List<DuplicateSubmissionResponse> Submissions { get; set; } = new();
        public decimal SimilarityScore { get; set; }
    }

    public class DuplicateSubmissionResponse
    {
        public Guid SubmissionId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class ImportSummaryResponse
    {
        public int TotalFiles { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public int DuplicatesFound { get; set; }
        public int ViolationsCreated { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    public class ImportJobsListResponse
    {
        public List<ImportJobSummaryResponse> Jobs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ImportJobSummaryResponse
    {
        public Guid JobId { get; set; }
        public ImportJobStatus Status { get; set; }
        public string ArchiveName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SemesterCode { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }
}