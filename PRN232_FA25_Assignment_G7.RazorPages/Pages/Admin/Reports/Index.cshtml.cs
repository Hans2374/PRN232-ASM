using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Reports;

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
    public List<SelectListItem> ExamSelectList { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public Guid? SelectedExamId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var examsResponse = await _apiClient.GetAsync<List<ExamResponse>>("/api/exams");
            Exams = examsResponse ?? new List<ExamResponse>();
            Exams = Exams.OrderByDescending(e => e.ExamDate).ToList();

            ExamSelectList = Exams.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} ({e.SubjectName}) - {e.ExamDate:MMM dd, yyyy}"
            }).ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exams for reports");
            TempData["Error"] = "Failed to load exams. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostExportAsync(Guid examId)
    {
        try
        {
            var response = await _apiClient.GetRawAsync($"/api/reports/exams/{examId}/results/export");
            
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = $"Exam_Results_{examId}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            var stream = await response.Content.ReadAsStreamAsync();
            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting exam results for {ExamId}", examId);
            TempData["Error"] = $"Failed to export results: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostExportViolationsAsync()
    {
        try
        {
            var response = await _apiClient.GetRawAsync("/api/reports/violations/export");
            
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = $"Violations_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            var stream = await response.Content.ReadAsStreamAsync();
            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting violations report");
            TempData["Error"] = $"Failed to export violations: {ex.Message}";
            return RedirectToPage();
        }
    }
}
