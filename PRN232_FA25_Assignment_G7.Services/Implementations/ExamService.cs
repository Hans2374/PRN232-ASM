using PRN232_FA25_Assignment_G7.Services.DTOs.Exam;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class ExamService : IExamService
{
    public Task<ExamDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ExamResponse> CreateAsync(CreateExamRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
