using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Submissions;

[Authorize(Roles = "Examiner")]
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApiClient apiClient, AuthSession authSession, ILogger<IndexModel> logger)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _logger = logger;
    }

    public PagedResult<SubmissionListDto>? Submissions { get; set; }
    public List<AssignedExamSummary>? AvailableExams { get; set; }
    public Guid? ExamId { get; set; }
    public string Status { get; set; } = "Pending";
    public int PageNumber { get; set; } = 1;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? examId, string status = "Pending", int page = 1)
    {
        if (!_authSession.IsInRole(HttpContext, "Examiner"))
        {
            return RedirectToPage("/Account/Login");
        }

        ExamId = examId;
        Status = status;
        PageNumber = page;

        try
        {
            // Get available exams for filter
            AvailableExams = await _apiClient.GetAsync<List<AssignedExamSummary>>("/api/examiner/exams");

            // Get submissions with filters
            var filter = new SubmissionFilter
            {
                ExamId = examId,
                Status = status,
                PageNumber = page,
                PageSize = 25
            };

            Submissions = await _apiClient.GetExaminerSubmissionsAsync(filter);

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized access to examiner submissions");
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading examiner submissions");
            ErrorMessage = "Unable to load submissions. Please try again later.";
            return Page();
        }
    }
}