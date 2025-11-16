using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record CreateSemesterRequest(
    [Required, StringLength(100)] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate
);

public record SemesterResponse(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate
);
