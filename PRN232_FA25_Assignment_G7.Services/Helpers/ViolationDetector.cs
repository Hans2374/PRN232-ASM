using System.Text.RegularExpressions;
using System.IO;

namespace PRN232_FA25_Assignment_G7.Services.Helpers;

public class ViolationDetector
{
    private static readonly (string pattern, string description)[] NamingRules = new[]
    {
        ("^[A-Za-z0-9_\\-]+$", "File or folder name contains invalid characters - only letters, numbers, underscore and hyphen allowed"),
        ("^[A-Z].*", "Top-level folder/file should start with an uppercase letter"),
        ("^.{1,100}$", "Name is too long or empty")
    };

    private static readonly string[] ForbiddenLines = new[]
    {
        "System.IO.File.WriteAllText",
        "Console.ReadLine",
        "Environment.Exit",
        "Thread.Abort",
        "Process.Start("
    };

    private static readonly string[] RequiredFunctionSignatures = new[]
    {
        "public static void Main",
        "public static int Main",
        "def main(",
        "public static void Solve",
    };

    /// <summary>
    /// Detects violations in submission files (naming rules and content rules).
    /// Returns list of detected violations.
    /// </summary>
    public async Task<List<DetectedViolation>> ScanSubmissionAsync(string extractedFolderPath, CancellationToken ct = default)
    {
        var violations = new List<DetectedViolation>();
        if (string.IsNullOrWhiteSpace(extractedFolderPath) || !Directory.Exists(extractedFolderPath))
            return violations;

        // Naming checks for files and folders (top-down)
        foreach (var path in Directory.EnumerateFileSystemEntries(extractedFolderPath, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            var name = Path.GetFileName(path);

            foreach (var rule in NamingRules)
            {
                if (!Regex.IsMatch(name, rule.pattern))
                {
                    violations.Add(new DetectedViolation("Naming", $"{rule.description}: '{name}'", 1, false));
                    break; // report once per file/folder
                }
            }

            await Task.Yield();
        }

        // Content checks for source files
        var codeFiles = Directory.EnumerateFiles(extractedFolderPath, "*.*", SearchOption.AllDirectories)
            .Where(f => new[] { ".cs", ".java", ".py", ".cpp", ".c", ".js", ".ts" }.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));

        foreach (var file in codeFiles)
        {
            ct.ThrowIfCancellationRequested();
            string content;
            try
            {
                content = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
            }
            catch
            {
                continue;
            }

            // Forbidden lines
            foreach (var pattern in ForbiddenLines)
            {
                if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(new DetectedViolation("ForbiddenLine", $"Forbidden usage '{pattern}' found in {Path.GetFileName(file)}", 5, true));
                }
            }

            // Missing required functions
            var ext = Path.GetExtension(file).ToLowerInvariant();
            var requires = ext switch
            {
                ".cs" => new[] { "public static void Main", "public static int Main", "public static void Solve" },
                ".java" => new[] { "public static void main(" },
                ".py" => new[] { "def main(" },
                _ => Array.Empty<string>()
            };

            if (requires.Length > 0 && !requires.Any(sig => content.Contains(sig)))
            {
                violations.Add(new DetectedViolation("MissingFunction", $"Missing required entry function in {Path.GetFileName(file)}", 3, false));
            }

            // Wrong template usage: simple heuristic - look for placeholders like TODO: TEMPLATE
            if (content.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase) || content.Contains("TODO TEMPLATE", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(new DetectedViolation("Template", $"Possible leftover template usage in {Path.GetFileName(file)}", 2, false));
            }

            // Unlock code heuristic: look for commented markers like UNLOCK or PROVIDEKEY
            if (content.Contains("UNLOCK", StringComparison.OrdinalIgnoreCase) || content.Contains("PROVIDEKEY", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(new DetectedViolation("UnlockCode", $"Potential unlock/key in source {Path.GetFileName(file)}", 5, true));
            }

            await Task.Yield();
        }

        return violations;
    }
}

public record DetectedViolation(string Type, string Description, int Severity, bool IsZeroScore);
