namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record ExaminerResponse(
    Guid Id,
    string FullName,
    string Email,
    List<string> AssignedSubjects
);

// Examiner Submission DTOs
public record SubmissionListDto(
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

public record ViolationDto(
    Guid Id,
    string Type,
    string Description,
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

public record SubmitGradeRequest(
    List<ScoreDto> Scores,
    string? FinalComment
);

// Double Grading DTOs
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

public record SubmitDoubleGradingRequest(
    List<ScoreDto> Scores,
    string? FinalComment
);
