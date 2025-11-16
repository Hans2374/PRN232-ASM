using PRN232_FA25_Assignment_G7.Services.DTOs.Semester;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class SemesterService : ISemesterService
{
    // TODO: Inject repository and implement logic
    public Task<SemesterResponse?> GetAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SemesterResponse>> GetActiveAsync(DateTime? onDate = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<SemesterResponse> CreateAsync(CreateSemesterRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
