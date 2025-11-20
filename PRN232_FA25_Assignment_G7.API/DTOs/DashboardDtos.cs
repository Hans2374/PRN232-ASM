namespace PRN232_FA25_Assignment_G7.API.DTOs;

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
    List<RecentSubmissionSummary> RecentSubmissions
);

public record RecentSubmissionSummary(
    Guid Id,
    string StudentCode,
    string ExamName,
    DateTime SubmittedAt,
    string Status
);

// Manager Dashboard DTOs
public record ManagerDashboardResponse(
    int TotalExams,
    int TotalExaminers,
    int TotalSubmissions,
    int GradedSubmissions,
    int PendingSubmissions,
    int DoubleGradeRequired,
    int ViolationsPending,
    List<ExamProgress> ExamsProgress
);

public record ExamProgress(
    Guid ExamId,
    string ExamName,
    int Graded,
    int Pending
);

// Moderator Dashboard DTOs
public record ModeratorDashboardResponse(
    int TotalExamsUnderReview,
    int TotalPendingComplaints,
    int TotalZeroScoreSubmissionsPending,
    List<ExamDiscrepancyDto> RecentDiscrepancies,
    List<TopIssueDto> TopIssues
);

public record ExamDiscrepancyDto(
    Guid SubmissionId,
    string ExamName,
    decimal ExaminerScore,
    decimal OtherScore,
    decimal Difference
);

public record TopIssueDto(
    string Type,
    int Count
);

// Examiner Dashboard DTOs
public record ExaminerDashboardResponse(
    int AssignedExamsCount,
    int PendingSubmissionsCount,
    int GradedSubmissionsCount,
    int PendingDoubleGradingCount,
    List<AssignedExamSummary> AssignedExams,
    List<RecentActivityDto> RecentActivities
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

public record RecentActivityDto(
    Guid SubmissionId,
    string Status,
    decimal? Score,
    DateTime Time
);
