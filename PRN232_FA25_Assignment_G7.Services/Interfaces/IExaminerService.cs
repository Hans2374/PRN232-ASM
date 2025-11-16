using PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IExaminerService
{
    Task<ExaminerResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExaminerResponse>> GetAllAsync(CancellationToken ct = default);
}
