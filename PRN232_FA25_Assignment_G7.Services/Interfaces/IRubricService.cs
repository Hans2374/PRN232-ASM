using PRN232_FA25_Assignment_G7.Services.DTOs.Rubric;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IRubricService
{
    Task<IReadOnlyList<RubricResponse>> GetByExamAsync(Guid examId, CancellationToken ct = default);
    Task<RubricResponse> AddCriterionAsync(AddRubricRequest request, CancellationToken ct = default);
}
