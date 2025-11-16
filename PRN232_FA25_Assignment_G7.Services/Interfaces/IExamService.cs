using PRN232_FA25_Assignment_G7.Services.DTOs.Exam;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IExamService
{
    Task<ExamDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<ExamResponse> CreateAsync(CreateExamRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
