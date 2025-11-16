using PRN232_FA25_Assignment_G7.Services.DTOs.Violation;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IViolationService
{
    Task<IReadOnlyList<ViolationResponse>> GetBySubmissionAsync(Guid submissionId, CancellationToken ct = default);
    Task<IReadOnlyList<ViolationResponse>> DetectAsync(Guid submissionId, CancellationToken ct = default);
}
