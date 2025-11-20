using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record CreateExamRequest(
    [Required] Guid SubjectId,
    [Required] Guid SemesterId,
    [Required, StringLength(200)] string Name,
    string? Description,
    [Required] DateTime ExamDate
);

public record ExamResponse(
    Guid Id,
    Guid SubjectId,
    string SubjectName,
    Guid SemesterId,
    string SemesterName,
    string Name,
    string? Description,
    DateTime ExamDate
);

public record ExamDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime ExamDate,
    SubjectResponse Subject,
    SemesterResponse Semester,
    List<RubricResponse> Rubrics,
    int TotalSubmissions
);

public record UpdateExamRequest(
    [Required] Guid SubjectId,
    [Required] Guid SemesterId,
    [Required, StringLength(200)] string Name,
    string? Description,
    [Required] DateTime ExamDate
);

public record AssignExaminerRequest(
    [Required] Guid ExaminerId,
    bool IsPrimaryGrader = true
);

public record ExamExaminerResponse(
    Guid ExamId,
    Guid ExaminerId,
    string ExaminerName,
    string ExaminerEmail,
    bool IsPrimaryGrader,
    DateTime AssignedAt
);
