using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Submissions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public IndexModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public Guid ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public List<SubmissionItem>? Submissions { get; set; }
        public List<ExamSummary>? ExamSummaries { get; set; }

        public class SubmissionItem
        {
            public Guid Id { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public DateTime SubmittedAt { get; set; }
            public string Status { get; set; } = string.Empty;
            public string ExaminerName { get; set; } = string.Empty;
            public bool HasViolations { get; set; }
        }

        public class ExamSummary
        {
            public Guid Id { get; set; }
            public string ExamName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public int TotalSubmissions { get; set; }
            public int PendingSubmissions { get; set; }
            public int GradedSubmissions { get; set; }
            public int FlaggedSubmissions { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid? examId = null)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            if (examId.HasValue && examId.Value != Guid.Empty)
            {
                // Show submissions for specific exam
                ExamId = examId.Value;

                try
                {
                    // Get exam name
                    var exam = await _apiClient.GetAsync<ExamInfo>($"/api/manager/exams/{examId}");
                    ExamName = exam?.ExamName ?? "Unknown Exam";

                    // Get submissions
                    Submissions = await _apiClient.GetAsync<List<SubmissionItem>>($"/api/manager/exams/{examId}/submissions");
                    return Page();
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["AuthError"] = "Your session has expired. Please log in again.";
                    return RedirectToPage("/Account/Login");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Failed to load submissions: {ex.Message}";
                    return Page();
                }
            }
            else
            {
                // Show exam summary list
                try
                {
                    ExamSummaries = await _apiClient.GetAsync<List<ExamSummary>>("/api/manager/submissions/summary");
                    if (ExamSummaries == null)
                    {
                        ExamSummaries = new List<ExamSummary>();
                    }
                    return Page();
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["AuthError"] = "Your session has expired. Please log in again.";
                    return RedirectToPage("/Account/Login");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Failed to load exam summaries: {ex.Message}";
                    return Page();
                }
            }
        }

        public class ExamInfo
        {
            public Guid Id { get; set; }
            public string ExamName { get; set; } = string.Empty;
        }
    }
}
