using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record AddRubricRequest(
    [Required] Guid ExamId,
    [Required, StringLength(500)] string Criteria,
    [Required, Range(1, 100)] int MaxScore
);

public record RubricResponse(
    Guid Id,
    Guid ExamId,
    string Criteria,
    int MaxScore
);
