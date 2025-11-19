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
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public DetailsModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public ViolationDetailDto? Violation { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!_authSession.IsExaminer(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Violation = await _apiClient.GetAsync<ViolationDetailDto>($"/api/violations/{id}");
                if (Violation == null)
                {
                    TempData["Error"] = "Violation not found.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Access denied.";
                Violation = null;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                Violation = null;
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach the server.";
                Violation = null;
            }

            return Page();
        }
    }

    public class ViolationDetailDto
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? Status { get; set; } // Pending Moderator, Pending Admin, Resolved
        public List<ViolationDecisionDto>? Decisions { get; set; }
    }

    public class ViolationDecisionDto
    {
        public string? Role { get; set; } // Examiner, Moderator, Admin
        public string? Decision { get; set; }
        public DateTime DecidedAt { get; set; }
    }
}