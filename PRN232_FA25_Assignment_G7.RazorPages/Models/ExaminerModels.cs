using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

// Examiner Dashboard
public class ExaminerDashboardResponse
{
    public int AssignedExamsCount { get; set; }
    public int PendingSubmissionsCount { get; set; }
    public int GradedSubmissionsCount { get; set; }
    public int PendingDoubleGradingCount { get; set; }
    public List<AssignedExamSummary> AssignedExams { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class AssignedExamSummary
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public int TotalSubmissions { get; set; }
    public int GradedSubmissions { get; set; }
    public bool IsPrimaryGrader { get; set; }
}

public class RecentActivityDto
{
    public Guid SubmissionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime Time { get; set; }
}

// Submissions
public class SubmissionFilter
{
    public Guid? ExamId { get; set; }
    public string Status { get; set; } = "Pending";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class SubmissionListDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ViolationsCount { get; set; }
}

public class SubmissionDetailDto
{
    public Guid SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public List<ViolationDto> Violations { get; set; } = new();
    public List<RubricCriterionDto> Rubric { get; set; } = new();
    public List<ScoreDto> Scores { get; set; } = new();
    public decimal? Score { get; set; }
    public string? FinalComment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RubricCriterionDto
{
    public Guid CriterionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
}

public class ScoreDto
{
    public Guid CriterionId { get; set; }
    public decimal Score { get; set; }
}

public class SubmitGradeRequest
{
    [Required]
    public List<ScoreDto> Scores { get; set; } = new();
    public string? FinalComment { get; set; }
}

// Double Grading
public class DoubleGradingFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class DoubleGradingTaskDto
{
    public Guid SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public decimal? PrimaryScore { get; set; }
    public decimal? SecondaryScore { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class DoubleGradingDetailDto
{
    public Guid SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public List<ViolationDto> Violations { get; set; } = new();
    public List<RubricCriterionDto> Rubric { get; set; } = new();
    public List<ScoreDto> Scores { get; set; } = new();
    public List<ScoreDto> PrimaryScores { get; set; } = new();
    public decimal? PrimaryScore { get; set; }
    public decimal? SecondaryScore { get; set; }
    public string? FinalComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class ViolationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SubmitDoubleGradingRequest
{
    public List<ScoreDto> Scores { get; set; } = new();
    public string? FinalComment { get; set; }
}

// Pagination
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}