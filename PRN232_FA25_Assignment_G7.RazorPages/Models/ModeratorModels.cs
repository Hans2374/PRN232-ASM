namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public enum ComplaintStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Escalated = 3
}

// Dashboard DTOs
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

// Complaints DTOs
public record ComplaintSummaryDto(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string ExamName,
    string Title,
    ComplaintStatus Status,
    DateTime CreatedAt
);

public record ComplaintDetailDto(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string ExamName,
    string Title,
    string Description,
    string? EvidencePath,
    ComplaintStatus Status,
    Guid? ReviewedBy,
    DateTime? ReviewedAt,
    string? ReviewComments,
    DateTime CreatedAt,
    List<ComplaintTimelineDto> Timeline
);

public record ComplaintTimelineDto(
    string Action,
    string Details,
    DateTime Timestamp
);

public record DecisionDto(
    string Decision, // "approve", "reject", "escalate"
    string Comment
);

// Zero-Score Verification DTOs
public record ZeroScoreSubmissionDto(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string ExamName,
    string ViolationType,
    string Reason,
    DateTime SubmissionTime
);

public record ZeroScoreDetailDto(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string ExamName,
    decimal? ExaminerScore,
    string? GradingComments,
    string ViolationType,
    string ViolationDescription,
    string? EvidencePath,
    DateTime CreatedAt
);

public record VerifyZeroScoreRequest(
    string Action, // "confirm" or "override"
    string ModeratorComment,
    decimal? OverrideScore // required if action == "override"
);