using DocumentFormat.OpenXml.Packaging;
using System.IO;

namespace PRN232_FA25_Assignment_G7.Services.Helpers;

public class WordImageExtractor
{
    /// <summary>
    /// Extracts images from Word documents (.docx) and saves to output folder.
    /// Returns list of saved image file paths.
    /// </summary>
    public async Task<List<string>> ExtractImagesFromWordAsync(string docxPath, string outputFolder, CancellationToken ct = default)
    {
        var results = new List<string>();
        if (string.IsNullOrWhiteSpace(docxPath) || !File.Exists(docxPath)) return results;
        Directory.CreateDirectory(outputFolder);

        try
        {
            using var doc = WordprocessingDocument.Open(docxPath, false);
            var imageParts = doc.MainDocumentPart?.ImageParts;
            if (imageParts == null) return results;

            int idx = 0;
            foreach (var imagePart in imageParts)
            {
                ct.ThrowIfCancellationRequested();
                var contentType = imagePart.ContentType; // e.g. image/png
                var ext = contentType.Split('/').LastOrDefault() ?? "bin";
                var fileName = Path.Combine(outputFolder, $"image_{idx++}.{ext}");
                using var stream = imagePart.GetStream();
                using var outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await stream.CopyToAsync(outStream, 81920, ct).ConfigureAwait(false);
                results.Add(fileName);
            }
        }
        catch
        {
            // ignore failures and return what we have
        }

        return results;
    }
}
