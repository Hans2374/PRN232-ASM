using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Violations
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

        public List<ViolationSummaryDto>? Violations { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsExaminer(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Violations = await _apiClient.GetAsync<List<ViolationSummaryDto>>("/api/violations?examinerId=me");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Access denied.";
                Violations = new List<ViolationSummaryDto>();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                Violations = new List<ViolationSummaryDto>();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach the server.";
                Violations = new List<ViolationSummaryDto>();
            }

            return Page();
        }
    }

    public class ViolationSummaryDto
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? Status { get; set; }
    }
}