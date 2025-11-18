using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Subjects;

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

    public List<SubjectResponse> Subjects { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? search)
    {
        try
        {
            SearchTerm = search ?? string.Empty;
            var response = await _apiClient.GetAsync<List<SubjectResponse>>("/api/subjects");
            Subjects = response ?? new List<SubjectResponse>();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Subjects = Subjects.Where(s =>
                    s.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subjects");
            TempData["Error"] = "Failed to load subjects. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var success = await _apiClient.DeleteAsync($"/api/subjects/{id}");
            if (success)
            {
                TempData["Success"] = "Subject deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete subject.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subject {Id}", id);
            TempData["Error"] = $"Error deleting subject: {ex.Message}";
        }

        return RedirectToPage();
    }
}
