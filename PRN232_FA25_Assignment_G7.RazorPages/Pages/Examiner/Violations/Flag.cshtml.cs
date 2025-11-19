using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Violations
{
    [Authorize(Roles = "Examiner")]
    public class FlagModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public FlagModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        [BindProperty]
        public ViolationReportInput Input { get; set; } = new();

        public Guid SubmissionId { get; set; }

        public class ViolationReportInput
        {
            public string? Reason { get; set; }
            // Optional file upload - for simplicity, assume string URL or base64, but here just reason
        }

        public IActionResult OnGetAsync(Guid submissionId)
        {
            if (!_authSession.IsExaminer(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            SubmissionId = submissionId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid submissionId)
        {
            if (!_authSession.IsExaminer(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var request = new
                {
                    submissionId = submissionId,
                    reason = Input.Reason
                    // Add file if needed
                };
                await _apiClient.PostAsync<object, object>("/api/violations/report", request);
                TempData["SuccessMessage"] = "Violation reported successfully.";
                return RedirectToPage("/Examiner/Submissions/Index");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Failed to report violation.";
                return Page();
            }
        }
    }
}