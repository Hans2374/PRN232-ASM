using PRN232_FA25_Assignment_G7.Services.DTOs.Submission;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface ISubmissionService
{
    Task<SubmissionResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<SubmissionResponse> ProcessUploadAsync(ProcessSubmissionRequest request, CancellationToken ct = default);
    Task<SubmissionScoringResult> ScoreAsync(Guid submissionId, CancellationToken ct = default);
    Task<ExportResultDto> ExportExamResultsAsync(Guid examId, CancellationToken ct = default);
}
