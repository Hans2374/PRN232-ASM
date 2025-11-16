namespace PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;

public record ExaminerResponse(Guid Id, string FullName, string Email, List<string> Subjects);
