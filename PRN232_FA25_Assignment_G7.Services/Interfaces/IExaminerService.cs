using PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;
using PRN232_FA25_Assignment_G7.Services.DTOs;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IExaminerService
{
    Task<ExaminerDashboardDto?> GetDashboardAsync(Guid examinerId, CancellationToken ct = default);
    Task<IReadOnlyList<ExaminerResponse>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<SubmissionSummaryDto>> GetAssignedSubmissionsAsync(Guid examinerId, SubmissionFilterDto filter, CancellationToken ct = default);
    Task<SubmissionDetailDto?> GetSubmissionDetailAsync(Guid submissionId, Guid examinerId, CancellationToken ct = default);
    Task<bool> SubmitGradeAsync(Guid submissionId, SubmitGradeDto dto, Guid examinerId, CancellationToken ct = default);
    Task<PagedResult<DoubleGradingTaskDto>> GetDoubleGradingTasksAsync(Guid examinerId, DoubleGradingFilterDto filter, CancellationToken ct = default);
    Task<DoubleGradingDetailDto?> GetDoubleGradingDetailAsync(Guid submissionId, Guid examinerId, CancellationToken ct = default);
    Task<bool> SubmitDoubleGradeAsync(Guid submissionId, SubmitDoubleGradingDto dto, Guid examinerId, CancellationToken ct = default);
}
