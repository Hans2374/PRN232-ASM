using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Violations;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<ViolationItem> Violations { get; set; } = new();

    public class ViolationItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int SubmissionId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public string ReviewStatus { get; set; } = string.Empty;
        public string? ReviewedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var violations = await _apiClient.GetAsync<List<ViolationItem>>("/api/violations");
            Violations = violations ?? new List<ViolationItem>();
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error loading violations: {ex.Message}";
            return Page();
        }
    }
}
