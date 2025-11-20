using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Verification
{
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ApiClient apiClient, AuthSession authSession, ILogger<DetailsModel> logger)
        {
            _apiClient = apiClient;
            _authSession = authSession;
            _logger = logger;
        }

        public ZeroScoreDetailDto? Detail { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Detail = await _apiClient.GetZeroScoreDetailAsync(id);
                if (Detail == null)
                {
                    TempData["Error"] = "Submission not found.";
                    return RedirectToPage("Index");
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading zero-score detail");
                TempData["Error"] = "Cannot reach server. Please try again later.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, string action, string moderatorComment, decimal? overrideScore)
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(moderatorComment))
            {
                TempData["Error"] = "Action and comment are required.";
                return RedirectToPage(new { id });
            }

            if (action == "override" && !overrideScore.HasValue)
            {
                TempData["Error"] = "Override score is required when overriding.";
                return RedirectToPage(new { id });
            }

            try
            {
                var request = new VerifyZeroScoreRequest(action, moderatorComment, overrideScore);
                await _apiClient.VerifyZeroScoreAsync(id, request);
                TempData["Success"] = "Verification submitted successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting verification");
                TempData["Error"] = "Failed to submit verification. Please try again.";
                return RedirectToPage(new { id });
            }

            return RedirectToPage("Index");
        }
    }
}