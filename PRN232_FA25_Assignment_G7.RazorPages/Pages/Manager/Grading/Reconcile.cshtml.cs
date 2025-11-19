using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Grading
{
    [Authorize]
    public class ReconcileModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public ReconcileModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public GradingReconciliationData? Data { get; set; }

        [BindProperty]
        public ReconciliationInput Input { get; set; } = new();

        public class GradingReconciliationData
        {
            public Guid SubmissionId { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public string ExamName { get; set; } = string.Empty;
            public decimal FirstExaminerScore { get; set; }
            public decimal SecondExaminerScore { get; set; }
            public string FirstExaminerName { get; set; } = string.Empty;
            public string SecondExaminerName { get; set; } = string.Empty;
        }

        public class ReconciliationInput
        {
            public decimal FinalScore { get; set; }
            public string Comments { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(Guid submissionId)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Data = await _apiClient.GetAsync<GradingReconciliationData>($"/api/manager/grading/{submissionId}");
                if (Data == null)
                {
                    TempData["Error"] = "Grading data not found.";
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

        public async Task<IActionResult> OnPostAsync(Guid submissionId)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(submissionId);
                return Page();
            }

            try
            {
                var request = new
                {
                    finalScore = Input.FinalScore,
                    comments = Input.Comments
                };
                await _apiClient.PostAsync<object, object>($"/api/manager/grading/{submissionId}/reconcile", request);
                TempData["SuccessMessage"] = "Grading reconciled successfully.";
                return RedirectToPage("/Manager/Grading/Index");
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
                TempData["Error"] = "Failed to reconcile grading.";
                return Page();
            }
        }
    }
}