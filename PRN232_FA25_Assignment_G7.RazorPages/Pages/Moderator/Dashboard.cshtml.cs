using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator
{
    public class DashboardModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public DashboardModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public ModeratorDashboardData? Data { get; set; }

        public class ModeratorDashboardData
        {
            public int ExamsAssignedForApproval { get; set; }
            public int PendingScoreApprovals { get; set; }
            public int PendingViolationReviews { get; set; }
            public int RecentlyResolvedItems { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Data = await _apiClient.GetAsync<ModeratorDashboardData>("/api/moderator/dashboard");
                if (Data == null)
                {
                    Data = new ModeratorDashboardData();
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load dashboard data: {ex.Message}";
            }

            return Page();
        }
    }
}
