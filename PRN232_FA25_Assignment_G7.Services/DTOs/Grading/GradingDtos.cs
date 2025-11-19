namespace PRN232_FA25_Assignment_G7.Services.DTOs.Grading;

public record GradeSubmissionRequest(
    decimal Score,
    string? Comments
);

public record DoubleGradeRequest(
    decimal SecondScore,
    string? Comments
);

public record AdjustScoreRequest(
    decimal NewScore,
    string Reason
);

public record SubmissionDetailResponse(
    Guid Id,
    Guid ExamId,
    string ExamName,
    string StudentCode,
    string OriginalFileName,
    string ExtractedFolderPath,
    decimal? Score,
    decimal? SecondScore,
    DateTime CreatedAt,
    string SubmissionStatus,
    string ReviewStatus,
    string? GradingComments,
    string? SecondGradingComments,
    string? ModeratorComments,
    string? AdminComments,
    DateTime? GradedAt,
    DateTime? SecondGradedAt,
    List<ViolationSummary> Violations
);

public record ViolationSummary(
    Guid Id,
    string Type,
    string Description,
    int Severity,
    bool IsZeroScore,
    string ReviewStatus
);

public record AssignExaminerRequest(
    Guid ExaminerId,
    bool IsPrimaryGrader
);

public record ExamExaminerResponse(
    Guid ExamId,
    Guid ExaminerId,
    string ExaminerName,
    string ExaminerEmail,
    bool IsPrimaryGrader,
    DateTime AssignedAt
);
