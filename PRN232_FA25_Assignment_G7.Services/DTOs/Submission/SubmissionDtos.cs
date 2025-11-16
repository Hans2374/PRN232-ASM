namespace PRN232_FA25_Assignment_G7.Services.DTOs.Submission;

public record ProcessSubmissionRequest(Guid ExamId, string StudentCode, string FilePath);
public record SubmissionResponse(Guid Id, Guid ExamId, string StudentCode, decimal? Score, DateTime CreatedAt);
public record SubmissionScoringResult(Guid SubmissionId, decimal? FinalScore, bool IsZeroScore, string Reason);
public record ExportResultDto(string ExamName, List<StudentScoreEntry> Scores);
public record StudentScoreEntry(string StudentCode, decimal? Score, bool HasViolations);
