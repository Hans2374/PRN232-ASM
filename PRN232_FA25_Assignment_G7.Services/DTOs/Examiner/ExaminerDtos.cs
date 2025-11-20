namespace PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;

public record ExaminerResponse(Guid Id, string FullName, string Email, List<string> Subjects);

// Dashboard
public record ExaminerDashboardDto(
    int AssignedExamsCount,
    int PendingSubmissionsCount,
    int GradedSubmissionsCount,
    int PendingDoubleGradingCount,
    List<AssignedExamDto> AssignedExams,
    List<RecentActivityDto> RecentActivities
);

public record AssignedExamDto(
    Guid ExamId,
    string ExamName,
    string SubjectName,
    DateTime ExamDate,
    int TotalSubmissions,
    int GradedSubmissions,
    bool IsPrimaryGrader
);

public record RecentActivityDto(
    Guid SubmissionId,
    string Status,
    decimal? Score,
    DateTime Time
);

// Submissions
public record SubmissionFilterDto
{
    public Guid? ExamId { get; set; }
    public string Status { get; set; } = "Pending";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public record SubmissionSummaryDto(
    Guid Id,
    Guid ExamId,
    string ExamName,
    string StudentName,
    string StudentCode,
    string OriginalFileName,
    decimal? Score,
    DateTime CreatedAt,
    int ViolationsCount
);

public record SubmissionDetailDto(
    Guid SubmissionId,
    string StudentName,
    string StudentCode,
    Guid ExamId,
    string ExamName,
    string FileUrl,
    List<ViolationDto> Violations,
    List<RubricCriterionDto> Rubric,
    List<ScoreDto> Scores,
    decimal? Score,
    string? FinalComment,
    DateTime CreatedAt
);

public record RubricCriterionDto(
    Guid CriterionId,
    string Description,
    decimal MaxScore
);

public record ScoreDto(
    Guid CriterionId,
    decimal Score
);

public record SubmitGradeDto(
    List<ScoreDto> Scores,
    string? FinalComment
);

// Double Grading
public record DoubleGradingFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public record DoubleGradingTaskDto(
    Guid SubmissionId,
    string StudentName,
    string StudentCode,
    string ExamName,
    decimal? PrimaryScore,
    decimal? SecondaryScore,
    DateTime AssignedAt
);

public record DoubleGradingDetailDto(
    Guid SubmissionId,
    string StudentName,
    string StudentCode,
    Guid ExamId,
    string ExamName,
    string FileUrl,
    List<ViolationDto> Violations,
    List<RubricCriterionDto> Rubric,
    List<ScoreDto> Scores,
    List<ScoreDto> PrimaryScores,
    decimal? PrimaryScore,
    decimal? SecondaryScore,
    string? FinalComment,
    DateTime CreatedAt,
    DateTime AssignedAt
);

public record SubmitDoubleGradingDto(
    List<ScoreDto> Scores,
    string? FinalComment
);

public record ViolationDto(
    Guid Id,
    string Type,
    string Description,
    DateTime CreatedAt
);
