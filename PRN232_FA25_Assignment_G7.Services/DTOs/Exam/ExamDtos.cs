namespace PRN232_FA25_Assignment_G7.Services.DTOs.Exam;

public record CreateExamRequest(Guid SubjectId, Guid SemesterId, string Name, string? Description, DateTime ExamDate);
public record ExamResponse(Guid Id, string Name, string? Description, DateTime ExamDate);
public record ExamDetailResponse(Guid Id, string Name, string? Description, DateTime ExamDate, int RubricCount, int SubmissionCount);
