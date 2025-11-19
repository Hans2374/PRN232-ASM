using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Grading
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

        public List<GradingOverviewItem>? GradingOverview { get; set; }

        public class GradingOverviewItem
        {
            public Guid ExamId { get; set; }
            public string ExamName { get; set; } = string.Empty;
            public int GradedSubmissions { get; set; }
            public int DoubleGradeRequired { get; set; }
            public int PendingManagerVerification { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                GradingOverview = await _apiClient.GetAsync<List<GradingOverviewItem>>("/api/manager/grading");
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
