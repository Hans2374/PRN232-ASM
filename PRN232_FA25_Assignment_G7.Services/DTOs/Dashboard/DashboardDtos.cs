namespace PRN232_FA25_Assignment_G7.Services.DTOs.Dashboard;

// Admin Dashboard DTOs
public record AdminDashboardResponse(
    int TotalUsers,
    int ActiveUsers,
    int TotalSubjects,
    int TotalSemesters,
    int TotalExams,
    int TotalSubmissions,
    int PendingViolations,
    int PendingReviews,
    List<RecentSubmission> RecentSubmissions
);

public record RecentSubmission(
    Guid Id,
    string StudentCode,
    string ExamName,
    DateTime SubmittedAt,
    string Status
);

// Manager Dashboard DTOs
public record ManagerDashboardResponse(
    int TotalExaminers,
    int TotalExams,
    int PendingGradingCount,
    int GradedCount,
    int PendingViolations,
    List<ExamGradingProgress> ExamProgress
);

public record ExamGradingProgress(
    Guid ExamId,
    string ExamName,
    int TotalSubmissions,
    int GradedSubmissions,
    decimal CompletionPercentage
);

// Moderator Dashboard DTOs
public record ModeratorDashboardResponse(
    int PendingViolationReviews,
    int ZeroScoreViolations,
    int ResolvedViolations,
    List<PendingViolationSummary> PendingViolations
);

public record PendingViolationSummary(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string ViolationType,
    int Severity,
    bool IsZeroScore,
    DateTime CreatedAt
);

// Examiner Dashboard DTOs
public record ExaminerDashboardResponse(
    int AssignedExamsCount,
    int PendingSubmissionsCount,
    int GradedSubmissionsCount,
    List<AssignedExamSummary> AssignedExams
);

public record AssignedExamSummary(
    Guid ExamId,
    string ExamName,
    string SubjectName,
    DateTime ExamDate,
    int TotalSubmissions,
    int GradedSubmissions,
    bool IsPrimaryGrader
);
