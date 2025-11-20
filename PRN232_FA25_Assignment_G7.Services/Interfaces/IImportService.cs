using PRN232_FA25_Assignment_G7.Services.DTOs;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces
{
    public interface IImportService
    {
        /// <summary>
        /// Starts a new import job for processing a RAR archive
        /// </summary>
        Task<Guid> StartImportJobAsync(Stream fileStream, string fileName, Guid subjectId, Guid semesterId, Guid examId, string initiatedBy);
        /// <summary>
        /// Starts a new import job using a file already saved on disk. Preferred for large uploads so the background
        /// task can open the file independently of the request lifetime.
        /// </summary>
        Task<Guid> StartImportJobAsync(string filePath, string fileName, Guid subjectId, Guid semesterId, Guid examId, string initiatedBy);

        /// <summary>
        /// Gets the current status of an import job
        /// </summary>
        Task<ImportStatusDto> GetImportStatusAsync(Guid jobId);

        /// <summary>
        /// Gets the complete results of a completed import job
        /// </summary>
        Task<ImportResultsDto> GetImportResultsAsync(Guid jobId);

        /// <summary>
        /// Gets a paginated list of import jobs
        /// </summary>
        Task<ImportJobsListDto> GetImportJobsAsync(int page, int pageSize);

        /// <summary>
        /// Cancels an import job if it's still in progress
        /// </summary>
        Task CancelImportJobAsync(Guid jobId);
    }
}