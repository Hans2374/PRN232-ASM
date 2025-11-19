namespace PRN232_FA25_Assignment_G7.API.DTOs;

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

public record ReportViolationRequest(
    Guid SubmissionId,
    string Type,
    string Description,
    int Severity,
    bool IsZeroScore
);

