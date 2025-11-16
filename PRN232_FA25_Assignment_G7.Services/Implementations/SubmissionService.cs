using PRN232_FA25_Assignment_G7.Services.DTOs.Submission;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class SubmissionService : ISubmissionService
{
    public Task<SubmissionResponse?> GetAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<SubmissionResponse> ProcessUploadAsync(ProcessSubmissionRequest request, CancellationToken ct = default)
    {
        // TODO: File extraction, duplicate detection, violation scanning
        throw new NotImplementedException();
    }

    public Task<SubmissionScoringResult> ScoreAsync(Guid submissionId, CancellationToken ct = default)
    {
        // TODO: Check violations, apply rubric, zero-score logic
        throw new NotImplementedException();
    }

    public Task<ExportResultDto> ExportExamResultsAsync(Guid examId, CancellationToken ct = default)
    {
        // TODO: Generate Excel export
        throw new NotImplementedException();
    }
}
