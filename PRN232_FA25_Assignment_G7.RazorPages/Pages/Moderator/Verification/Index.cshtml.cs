using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Verification
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApiClient apiClient, AuthSession authSession, ILogger<IndexModel> logger)
        {
            _apiClient = apiClient;
            _authSession = authSession;
            _logger = logger;
        }

        public PagedResult<ZeroScoreSubmissionDto>? Submissions { get; set; }
        public string Status { get; set; } = "Pending";
        public string? StudentCode { get; set; }
        public int CurrentPage { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(string status = "Pending", string? studentCode = null, int page = 1)
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            Status = status;
            StudentCode = studentCode;
            CurrentPage = page;

            try
            {
                Submissions = await _apiClient.GetZeroScoreSubmissionsAsync(null, status, studentCode, page, 25);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading zero-score submissions");
                TempData["Error"] = "Cannot reach server. Please try again later.";
            }

            return Page();
        }
    }
}