using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Semesters;

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

    public List<SemesterResponse> Semesters { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync<List<SemesterResponse>>("/api/semesters");
            Semesters = response ?? new List<SemesterResponse>();
            Semesters = Semesters.OrderByDescending(s => s.StartDate).ToList();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching semesters");
            TempData["Error"] = "Failed to load semesters. Please try again.";
            return Page();
        }
    }
}
