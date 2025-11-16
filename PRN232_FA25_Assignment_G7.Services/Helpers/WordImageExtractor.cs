namespace PRN232_FA25_Assignment_G7.Services.Helpers;

public class WordImageExtractor
{
    /// <summary>
    /// Extracts images from Word documents (for assignments that include diagrams).
    /// TODO: Implement using OpenXML SDK or similar
    /// </summary>
    public async Task<List<string>> ExtractImagesFromWordAsync(string docxPath, string outputFolder, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        // Extract images from .docx and save to output folder
        return new List<string>(); // Return list of extracted image paths
    }
}
