using System.Text.RegularExpressions;

namespace PRN232_FA25_Assignment_G7.Services.Helpers;

public class DuplicateChecker
{
    /// <summary>
    /// Token-based similarity using Jaccard index (simple and fast for many files).
    /// Returns similarity0.0 -1.0
    /// </summary>
    public async Task<double> CalculateSimilarityAsync(string filePath1, string filePath2, CancellationToken ct = default)
    {
        var tokens1 = await TokenizeFileAsync(filePath1, ct).ConfigureAwait(false);
        var tokens2 = await TokenizeFileAsync(filePath2, ct).ConfigureAwait(false);

        if (tokens1.Count == 0 && tokens2.Count == 0) return 1.0;
        if (tokens1.Count == 0 || tokens2.Count == 0) return 0.0;

        var intersect = tokens1.Intersect(tokens2).Count();
        var union = tokens1.Union(tokens2).Count();
        return union == 0 ? 0.0 : (double)intersect / union;
    }

    public async Task<List<(Guid SubmissionId, double Similarity)>> FindDuplicatesAsync(Guid submissionId, List<Guid> otherSubmissionIds, CancellationToken ct = default)
    {
        // This method requires access to the repository / file paths to compare submissions.
        // Leave unimplemented here; the service layer should call CalculateSimilarityAsync for file pairs.
        await Task.CompletedTask;
        return new List<(Guid, double)>();
    }

    private static async Task<HashSet<string>> TokenizeFileAsync(string path, CancellationToken ct)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return set;

        string content;
        try
        {
            content = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        }
        catch
        {
            return set;
        }

        // Remove strings and comments quickly (basic)
        content = Regex.Replace(content, @"\"".*?""", string.Empty);
        content = Regex.Replace(content, @"//.*?$", string.Empty, RegexOptions.Multiline);
        content = Regex.Replace(content, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

        // Tokenize by non-word chars
        var tokens = Regex.Split(content, @"\W+").Where(t => t.Length >= 2 && t.Length <= 30);
        foreach (var t in tokens)
        {
            set.Add(t.ToLowerInvariant());
        }

        return set;
    }

    // Optional Levenshtein for small strings
    public static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
        if (string.IsNullOrEmpty(b)) return a.Length;

        var d = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) d[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }

        return d[a.Length, b.Length];
    }
}
