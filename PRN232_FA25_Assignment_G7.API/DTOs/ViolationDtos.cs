namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record ViolationResponse(
    Guid Id,
    Guid SubmissionId,
    string StudentCode,
    string Type,
    string Description,
    int Severity,
    bool IsZeroScore
);
