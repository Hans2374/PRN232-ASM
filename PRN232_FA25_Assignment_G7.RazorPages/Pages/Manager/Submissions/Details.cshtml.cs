using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Submissions
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

        public SubmissionDetails? Submission { get; set; }
        public List<GradingHistoryItem>? GradingHistory { get; set; }

        public class SubmissionDetails
        {
            public Guid Id { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public string ExamName { get; set; } = string.Empty;
            public DateTime SubmittedAt { get; set; }
            public string Status { get; set; } = string.Empty;
            public string FileUrl { get; set; } = string.Empty;
            public string FileType { get; set; } = string.Empty;
        }

        public class GradingHistoryItem
        {
            public Guid Id { get; set; }
            public string ExaminerName { get; set; } = string.Empty;
            public DateTime GradedAt { get; set; }
            public decimal Score { get; set; }
            public string Comments { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Submission = await _apiClient.GetAsync<SubmissionDetails>($"/api/manager/submissions/{id}");
                GradingHistory = await _apiClient.GetAsync<List<GradingHistoryItem>>($"/api/manager/submissions/{id}/grading-history");

                if (Submission == null)
                {
                    TempData["Error"] = "Submission not found.";
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