using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Violations
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public IndexModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public List<ViolationItem>? Violations { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(SearchTerm))
                    queryParams["search"] = SearchTerm;
                if (!string.IsNullOrEmpty(StatusFilter))
                    queryParams["status"] = StatusFilter;

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}")) : "";
                Violations = await _apiClient.GetAsync<List<ViolationItem>>($"/api/moderator/violations{queryString}");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load violations: {ex.Message}";
            }

            return Page();
        }
    }

    public class ViolationItem
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string? ExaminerName { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public DateTime ReportedAt { get; set; }
    }
}
