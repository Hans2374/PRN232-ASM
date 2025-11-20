namespace PRN232_FA25_Assignment_G7.API.DTOs;

using PRN232_FA25_Assignment_G7.Repositories.Entities;

public record ViolationResponse(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string Type,
    string Description,
    int Severity,
    bool IsZeroScore
);

public record ViolationDetailResponse(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string ExamName,
    string Type,
    string Description,
    int Severity,
    bool IsZeroScore,
    string ReviewStatus,
    Guid? ReviewedBy,
    DateTime? ReviewedAt,
    string? ReviewComments,
    DateTime CreatedAt
);

public record ReviewViolationRequest(
    bool Approve,
    string? Comments
);

public record ConfirmZeroScoreRequest(
    bool Confirm,
    string? Comments
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

public record ReportViolationRequest(
    Guid SubmissionId,
    string Type,
    string Description,
    int Severity,
    bool IsZeroScore
);

