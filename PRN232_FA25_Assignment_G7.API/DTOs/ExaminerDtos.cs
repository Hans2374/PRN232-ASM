namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record ExaminerResponse(
    Guid Id,
    string FullName,
    string Email,
    List<string> AssignedSubjects
);
