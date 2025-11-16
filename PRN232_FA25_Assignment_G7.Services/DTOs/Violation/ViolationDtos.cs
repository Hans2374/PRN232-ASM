namespace PRN232_FA25_Assignment_G7.Services.DTOs.Violation;

public record ViolationResponse(Guid Id, Guid SubmissionId, string Type, string Description, int Severity, bool IsZeroScore);
