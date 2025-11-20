using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.BulkUpload;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;

    public IndexModel(ApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
    }

    public List<ExamSummary> Exams { get; set; } = new();

    public class ExamSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check authorization
        if (!_authSession.IsInRole(HttpContext, "Admin") &&
            !_authSession.IsInRole(HttpContext, "Manager"))
        {
            TempData["AuthError"] = "You don't have permission to access this page.";
            return RedirectToPage("/Index");
        }

        try
        {
            // Load available exams from API
            var exams = await _apiClient.GetAsync<List<ExamSummary>>("api/exams");
            Exams = exams ?? new List<ExamSummary>();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to load exams: {ex.Message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // This page doesn't handle POST directly - the form submits to the API
        return RedirectToPage();
    }
}