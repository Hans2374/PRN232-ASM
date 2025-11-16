using PRN232_FA25_Assignment_G7.Services.DTOs.Subject;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface ISubjectService
{
    Task<SubjectResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SubjectResponse>> GetAllAsync(CancellationToken ct = default);
    Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default);
    Task<SubjectResponse?> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
