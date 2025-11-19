using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Exams
{
    [Authorize(Roles = "Examiner")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public IndexModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public List<AssignedExamDto>? Exams { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsExaminer(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Exams = await _apiClient.GetAsync<List<AssignedExamDto>>("/api/examiner/exams");
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach the server.";
            }

            return Page();
        }
    }

    public class AssignedExamDto
    {
        public Guid Id { get; set; }
        public string? ExamName { get; set; }
        public string? SubjectName { get; set; }
        public string? SemesterName { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public string? GradingProgress { get; set; } // e.g., "50% Complete"
    }
}
