namespace PRN232_FA25_Assignment_G7.Services.Helpers;

public class FileProcessingHelper
{
    /// <summary>
    /// Extracts uploaded ZIP files and prepares for analysis.
    /// TODO: Implement ZIP extraction, folder organization
    /// </summary>
    public async Task<string> ExtractSubmissionAsync(string uploadedFilePath, string destinationFolder, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        // Extract ZIP to destination
        // Return extracted folder path
        return destinationFolder;
    }

    public async Task<List<string>> GetAllCodeFilesAsync(string folderPath, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        // Recursively find .cs, .java, .py, etc.
        return new List<string>();
    }
}
