using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator
{
    public class DashboardModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ApiClient apiClient, AuthSession authSession, ILogger<DashboardModel> logger)
        {
            _apiClient = apiClient;
            _authSession = authSession;
            _logger = logger;
        }

        public ModeratorDashboardResponse? Data { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Data = await _apiClient.GetModeratorDashboardAsync();
                if (Data == null)
                {
                    TempData["Error"] = "Unable to load dashboard data.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading moderator dashboard");
                TempData["Error"] = "Cannot reach server. Please try again later.";
            }

            return Page();
        }
    }
}
