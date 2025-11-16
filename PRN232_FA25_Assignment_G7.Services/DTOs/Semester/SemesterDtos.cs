namespace PRN232_FA25_Assignment_G7.Services.DTOs.Semester;

public record CreateSemesterRequest(string Name, DateTime StartDate, DateTime EndDate);
public record SemesterResponse(Guid Id, string Name, DateTime StartDate, DateTime EndDate);
