using PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class ExaminerService : IExaminerService
{
    public Task<ExaminerResponse?> GetAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ExaminerResponse>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
