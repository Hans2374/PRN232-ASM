using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Examiners
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

        public List<ExaminerItem>? Examiners { get; set; }

        public class ExaminerItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public int AssignedExamsCount { get; set; }
            public int TotalGradedSubmissions { get; set; }
            public string Status { get; set; } = string.Empty; // Active, Disabled
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Examiners = await _apiClient.GetAsync<List<ExaminerItem>>("/api/manager/examiners");
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
