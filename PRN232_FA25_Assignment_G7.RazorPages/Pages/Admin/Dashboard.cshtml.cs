using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using System.Security.Claims;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin
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

        public AdminDashboardData? Dashboard { get; set; }
        public UserInfo? CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsInRole(HttpContext, "Admin"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            CurrentUser = new UserInfo
            {
                FullName = User.FindFirst("FullName")?.Value ?? "Unknown",
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown",
                Username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown"
            };

            try
            {
                Dashboard = await _apiClient.GetAsync<AdminDashboardData>("/api/admin/dashboard");
                if (Dashboard == null)
                {
                    Dashboard = new AdminDashboardData();
                }
                if (Dashboard.RecentSubmissions == null)
                {
                    Dashboard.RecentSubmissions = new List<RecentSubmission>();
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

    public class AdminDashboardData
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalSemesters { get; set; }
        public int TotalExams { get; set; }
        public int TotalSubmissions { get; set; }
        public int PendingReviews { get; set; }
        public int PendingViolations { get; set; }
        public List<RecentSubmission>? RecentSubmissions { get; set; } = new List<RecentSubmission>();
    }

    public class RecentSubmission
    {
        public string? StudentCode { get; set; }
        public string? ExamName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? Status { get; set; }
    }

    public class UserInfo
    {
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? Username { get; set; }
    }
}