using Microsoft.AspNetCore.SignalR;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.SignalR
{
    public class ImportHub : Hub
    {
        public async Task SubscribeToJobUpdates(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"import-job-{jobId}");
        }

        public async Task UnsubscribeFromJobUpdates(string jobId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"import-job-{jobId}");
        }

        public async Task SubscribeToImportJobs()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "import-jobs");
        }

        public async Task UnsubscribeFromImportJobs()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "import-jobs");
        }
    }

    public static class ImportHubExtensions
    {
        public static async Task NotifyJobStatusUpdate(this IHubContext<ImportHub> hub, Guid jobId, ImportJobStatus status, string message)
        {
            await hub.Clients.Group($"import-job-{jobId}").SendAsync("JobStatusUpdate", new
            {
                JobId = jobId,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        public static async Task NotifyJobProgress(this IHubContext<ImportHub> hub, Guid jobId, int processed, int total)
        {
            await hub.Clients.Group($"import-job-{jobId}").SendAsync("JobProgress", new
            {
                JobId = jobId,
                ProcessedFiles = processed,
                TotalFiles = total,
                Progress = total > 0 ? (double)processed / total : 0,
                Timestamp = DateTime.UtcNow
            });
        }

        public static async Task NotifyNewJob(this IHubContext<ImportHub> hub, Guid jobId, string archiveName)
        {
            await hub.Clients.Group("import-jobs").SendAsync("NewImportJob", new
            {
                JobId = jobId,
                ArchiveName = archiveName,
                Timestamp = DateTime.UtcNow
            });
        }

        public static async Task NotifyJobCompleted(this IHubContext<ImportHub> hub, Guid jobId, ImportJobStatus finalStatus)
        {
            await hub.Clients.Group($"import-job-{jobId}").SendAsync("JobCompleted", new
            {
                JobId = jobId,
                FinalStatus = finalStatus,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}