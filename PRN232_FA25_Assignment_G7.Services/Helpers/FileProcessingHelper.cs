using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PRN232_FA25_Assignment_G7.Services.Helpers;

public class FileProcessingHelper
{
    private static readonly string[] CodeExtensions = new[] { ".cs", ".java", ".py", ".cpp", ".c", ".js", ".ts", ".cshtml", ".html", ".css", ".rb", ".go", ".php" };

    /// <summary>
    /// Extracts uploaded ZIP/RAR files and prepares for analysis.
    /// - Uses SharpCompress to support multiple archive formats
    /// - Prevents Zip-Slip by validating entry paths
    /// - Preserves folder structure under destinationFolder
    /// - Returns the root extracted folder path
    /// </summary>
    public async Task<string> ExtractSubmissionAsync(string uploadedFilePath, string destinationFolder, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(uploadedFilePath)) throw new ArgumentNullException(nameof(uploadedFilePath));
        if (string.IsNullOrWhiteSpace(destinationFolder)) throw new ArgumentNullException(nameof(destinationFolder));

        // Ensure destination folder exists
        Directory.CreateDirectory(destinationFolder);

        // Use a unique subfolder per extraction to avoid collisions when processing many submissions in batch
        var extractedRoot = Path.Combine(destinationFolder, Path.GetFileNameWithoutExtension(uploadedFilePath) + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
        Directory.CreateDirectory(extractedRoot);

        // Open archive using SharpCompress (supports .zip, .rar, .7z, tar, etc.)
        using var stream = File.OpenRead(uploadedFilePath);
        using var archive = ArchiveFactory.Open(stream);

        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            ct.ThrowIfCancellationRequested();

            // Normalize entry key / path
            var entryKey = entry.Key.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            // Prevent Zip-Slip: ensure the destination path is within the extractedRoot
            var destinationPath = Path.GetFullPath(Path.Combine(extractedRoot, entryKey));
            var extractedRootFull = Path.GetFullPath(extractedRoot);
            if (!destinationPath.StartsWith(extractedRootFull + Path.DirectorySeparatorChar) && !destinationPath.Equals(extractedRootFull, StringComparison.OrdinalIgnoreCase))
            {
                // Skip dangerous entry
                continue;
            }

            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir)) Directory.CreateDirectory(destinationDir);

            // Extract entry asynchronously
            using var entryStream = entry.OpenEntryStream();
            // Use FileStream with async copy
            using var outStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await entryStream.CopyToAsync(outStream, 81920, ct).ConfigureAwait(false);
        }

        return extractedRoot;
    }

    public async Task<List<string>> GetAllCodeFilesAsync(string folderPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(folderPath)) throw new ArgumentNullException(nameof(folderPath));
        if (!Directory.Exists(folderPath)) return new List<string>();

        var results = new List<string>();

        // Use a stack to avoid recursion overhead for deep folder structures when handling hundreds of submissions
        var dirs = new Stack<string>();
        dirs.Push(folderPath);

        while (dirs.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var current = dirs.Pop();

            // Enumerate directories first
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(current))
                {
                    dirs.Push(dir);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }

            // Enumerate files and filter by code extensions
            try
            {
                foreach (var file in Directory.EnumerateFiles(current))
                {
                    ct.ThrowIfCancellationRequested();
                    var ext = Path.GetExtension(file);
                    if (!string.IsNullOrEmpty(ext) && CodeExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                    {
                        results.Add(file);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip files we can't access
            }

            // Yield to caller to avoid blocking for long-running enumeration
            await Task.Yield();
        }

        return results;
    }
}
