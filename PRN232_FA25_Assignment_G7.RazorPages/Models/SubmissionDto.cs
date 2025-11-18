namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public class SubmissionResponse
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ExtractedFolderPath { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ViolationCount { get; set; }
}

public class SubmissionDetailResponse
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ViolationResponse> Violations { get; set; } = new();
}

public class StudentResult
{
    public string StudentCode { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public bool IsZeroScore { get; set; }
    public string ZeroScoreReason { get; set; } = string.Empty;
    public List<string> Violations { get; set; } = new();
}
