namespace PRN232_FA25_Assignment_G7.Services.DTOs.Rubric;

public record AddRubricRequest(Guid ExamId, string Criteria, int MaxScore);
public record RubricResponse(Guid Id, Guid ExamId, string Criteria, int MaxScore);
