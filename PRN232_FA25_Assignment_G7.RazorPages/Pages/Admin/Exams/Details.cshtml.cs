using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Exams;

[Authorize(Policy = "AdminOrManager")]
public class DetailsModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ApiClient apiClient, ILogger<DetailsModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public ExamDetailResponse? Exam { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            Exam = await _apiClient.GetAsync<ExamDetailResponse>($"/api/exams/{id}");
            
            if (Exam == null)
            {
                TempData["Error"] = "Exam not found.";
                return RedirectToPage("Index");
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading exam {Id}", id);
            TempData["Error"] = "Failed to load exam details.";
            return RedirectToPage("Index");
        }
    }
}
