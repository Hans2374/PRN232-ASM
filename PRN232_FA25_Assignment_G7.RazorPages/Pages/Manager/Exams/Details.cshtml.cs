using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Exams
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

        public ExamDetails? Exam { get; set; }
        public List<AssignedExaminer>? Examiners { get; set; }
        public List<SubmissionSummary>? Submissions { get; set; }

        public class ExamDetails
        {
            public Guid Id { get; set; }
            public string ExamName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public string SemesterName { get; set; } = string.Empty;
            public DateTime ExamDate { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public class AssignedExaminer
        {
            public Guid ExaminerId { get; set; }
            public string ExaminerName { get; set; } = string.Empty;
            public bool IsPrimaryGrader { get; set; }
        }

        public class SubmissionSummary
        {
            public Guid Id { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime SubmittedAt { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Exam = await _apiClient.GetAsync<ExamDetails>($"/api/manager/exams/{id}");
                Examiners = await _apiClient.GetAsync<List<AssignedExaminer>>($"/api/manager/exams/{id}/examiners");
                Submissions = await _apiClient.GetAsync<List<SubmissionSummary>>($"/api/manager/exams/{id}/submissions");

                if (Exam == null)
                {
                    TempData["Error"] = "Exam not found.";
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