using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Exams;

[Authorize(Policy = "AdminOrManager")]
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApiClient apiClient, ILogger<IndexModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public List<ExamResponse> Exams { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? search)
    {
        try
        {
            SearchTerm = search ?? string.Empty;
            var response = await _apiClient.GetAsync<List<ExamResponse>>("/api/exams");
            Exams = response ?? new List<ExamResponse>();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Exams = Exams.Where(e =>
                    e.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.SubjectName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.SemesterName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            Exams = Exams.OrderByDescending(e => e.ExamDate).ToList();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exams");
            TempData["Error"] = "Failed to load exams. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var success = await _apiClient.DeleteAsync($"/api/exams/{id}");
            if (success)
            {
                TempData["Success"] = "Exam deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete exam.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exam {Id}", id);
            TempData["Error"] = $"Error deleting exam: {ex.Message}";
        }

        return RedirectToPage();
    }
}
