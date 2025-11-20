using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record ProcessSubmissionRequest(
    [Required] Guid ExamId,
    [Required, StringLength(50)] string StudentCode,
    [Required] IFormFile File
);

public record SubmissionResponse(
    Guid Id,
    Guid ExamId,
    string ExamName,
    string StudentCode,
    string OriginalFileName,
    string ExtractedFolderPath,
    decimal? Score,
    DateTime CreatedAt,
    int ViolationCount
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
    int ViolationCount
);

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

public record SubmissionScoringResult(
    Guid SubmissionId,
    decimal? Score,
    bool IsZeroScore,
    string Reason
);

public record ExportResultDto(
    string ExamName,
    DateTime ExportedAt,
    List<StudentResult> Results
);

public record StudentResult(
    string StudentCode,
    decimal? Score,
    bool IsZeroScore,
    string ZeroScoreReason,
    List<string> Violations
);
