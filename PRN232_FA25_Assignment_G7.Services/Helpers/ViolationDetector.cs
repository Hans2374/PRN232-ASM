namespace PRN232_FA25_Assignment_G7.Services.Helpers;

public class ViolationDetector
{
    /// <summary>
    /// Detects violations in submission files (e.g., disallowed libraries, patterns, file types).
    /// TODO: Implement rule-based scanning
    /// </summary>
    public async Task<List<DetectedViolation>> ScanSubmissionAsync(string extractedFolderPath, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        var violations = new List<DetectedViolation>();

        // Example: scan for forbidden imports, file extensions, etc.
        // violations.Add(new DetectedViolation("Plagiarism", "Detected code similarity > 80%", 5, true));

        return violations;
    }
}

public record DetectedViolation(string Type, string Description, int Severity, bool IsZeroScore);
