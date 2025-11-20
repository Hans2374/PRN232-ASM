using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager
{
    [Authorize(Roles = "Manager")]
    public class DashboardModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public DashboardModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public ManagerDashboardData? Data { get; set; }

        public class ManagerDashboardData
        {
            public int TotalExams { get; set; }
            public int TotalExaminers { get; set; }
            public int TotalSubmissions { get; set; }
            public int GradedSubmissions { get; set; }
            public int PendingSubmissions { get; set; }
            public int DoubleGradeRequired { get; set; }
            public int ViolationsPending { get; set; }
            public List<ExamProgress> ExamsProgress { get; set; } = new();
        }

        public class ExamProgress
        {
            public Guid ExamId { get; set; }
            public string ExamName { get; set; } = string.Empty;
            public int Graded { get; set; }
            public int Pending { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Data = await _apiClient.GetAsync<ManagerDashboardData>("/api/manager/dashboard");
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
