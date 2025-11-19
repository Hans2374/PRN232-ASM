using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Examiners
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public DetailsModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public ExaminerDetails? Examiner { get; set; }

        public class ExaminerDetails
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public List<AssignedExam> AssignedExams { get; set; } = new();
            public PerformanceStats Performance { get; set; } = new();
        }

        public class AssignedExam
        {
            public Guid ExamId { get; set; }
            public string ExamName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public DateTime ExamDate { get; set; }
            public int TotalSubmissions { get; set; }
            public int GradedSubmissions { get; set; }
        }

        public class PerformanceStats
        {
            public int TotalGraded { get; set; }
            public double AverageScore { get; set; }
            public int ViolationsReported { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Examiner = await _apiClient.GetAsync<ExaminerDetails>($"/api/manager/examiners/{id}");
                if (Examiner == null)
                {
                    TempData["Error"] = "Examiner not found.";
                }
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
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
                TempData["Error"] = "Cannot reach server.";
                return Page();
            }
        }
    }
}