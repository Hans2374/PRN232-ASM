using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Complaints
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

        public ComplaintDetailDto? Complaint { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Complaint = await _apiClient.GetModeratorComplaintAsync(id);
                if (Complaint == null)
                {
                    TempData["Error"] = "Complaint not found.";
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
                _logger.LogError(ex, "Error loading complaint details");
                TempData["Error"] = "Cannot reach server. Please try again later.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, string decision, string comment)
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(decision) || string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Decision and comment are required.";
                return RedirectToPage(new { id });
            }

            try
            {
                var dto = new DecisionDto(decision, comment);

                await _apiClient.DecideModeratorComplaintAsync(id, dto);
                TempData["Success"] = "Complaint decision submitted successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting complaint decision");
                TempData["Error"] = "Failed to submit decision. Please try again.";
                return RedirectToPage(new { id });
            }

            return RedirectToPage("Index");
        }
    }
}