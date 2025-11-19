using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Scores
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

        public List<ScoreItem>? Scores { get; set; }

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
                Scores = await _apiClient.GetAsync<List<ScoreItem>>($"/api/moderator/scores{queryString}");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load scores: {ex.Message}";
            }

            return Page();
        }
    }

    public class ScoreItem
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string? StudentId { get; set; }
        public decimal ExaminerScore { get; set; }
        public decimal? AutoScore { get; set; }
        public string? Status { get; set; }
        public string? ExamName { get; set; }
    }
}