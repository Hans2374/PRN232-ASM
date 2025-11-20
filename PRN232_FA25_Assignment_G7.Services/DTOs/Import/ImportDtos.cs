using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.Services.DTOs
{
    public record ViolationDto(
        Guid Id,
        string Type,
        string Description,
        DateTime CreatedAt
    );

    public class ImportStatusDto
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

    public class ImportResultsDto
    {
        public Guid JobId { get; set; }
        public ImportJobStatus Status { get; set; }
        public List<ImportResultItemDto> SuccessfulImports { get; set; } = new();
        public List<ImportFailureDto> FailedImports { get; set; } = new();
        public List<DuplicateGroupDto> DuplicateGroups { get; set; } = new();
        public List<ViolationDto> Violations { get; set; } = new();
        public ImportSummaryDto Summary { get; set; } = null!;
    }

    public class ImportResultItemDto
    {
        public Guid SubmissionId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public DateTime ImportedAt { get; set; }
    }

    public class ImportFailureDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
    }

    public class DuplicateGroupDto
    {
        public Guid GroupId { get; set; }
        public List<DuplicateSubmissionDto> Submissions { get; set; } = new();
        public decimal SimilarityScore { get; set; }
    }

    public class DuplicateSubmissionDto
    {
        public Guid SubmissionId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class ImportSummaryDto
    {
        public int TotalFiles { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public int DuplicatesFound { get; set; }
        public int ViolationsCreated { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    public class ImportJobsListDto
    {
        public List<ImportJobSummaryDto> Jobs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ImportJobSummaryDto
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