namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public class ViolationResponse
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool IsZeroScore { get; set; }
}
