using PRN232_FA25_Assignment_G7.Services.DTOs.Rubric;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class RubricService : IRubricService
{
    public Task<IReadOnlyList<RubricResponse>> GetByExamAsync(Guid examId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<RubricResponse> AddCriterionAsync(AddRubricRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
