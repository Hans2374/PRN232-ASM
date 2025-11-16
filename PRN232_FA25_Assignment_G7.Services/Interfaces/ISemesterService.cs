using PRN232_FA25_Assignment_G7.Services.DTOs.Semester;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface ISemesterService
{
    Task<SemesterResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SemesterResponse>> GetActiveAsync(DateTime? onDate = null, CancellationToken ct = default);
    Task<SemesterResponse> CreateAsync(CreateSemesterRequest request, CancellationToken ct = default);
}
