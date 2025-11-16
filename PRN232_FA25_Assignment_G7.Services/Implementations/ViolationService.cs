using PRN232_FA25_Assignment_G7.Services.DTOs.Violation;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class ViolationService : IViolationService
{
    public Task<IReadOnlyList<ViolationResponse>> GetBySubmissionAsync(Guid submissionId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ViolationResponse>> DetectAsync(Guid submissionId, CancellationToken ct = default)
    {
        // TODO: Run duplicate checker, violation detector
        throw new NotImplementedException();
    }
}
