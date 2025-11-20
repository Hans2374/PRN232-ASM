using PRN232_FA25_Assignment_G7.Services.DTOs.Submission;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IBulkUploadService
{
    Task<BulkUploadResult> ProcessBulkUploadAsync(BulkUploadRequest request, IProgress<BulkUploadProgress> progress, CancellationToken ct = default);
}