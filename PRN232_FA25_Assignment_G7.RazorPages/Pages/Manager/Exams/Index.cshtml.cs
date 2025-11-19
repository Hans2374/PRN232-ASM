using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Exams
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

        public List<ExamItem>? Exams { get; set; }

        public class ExamItem
        {
            public Guid Id { get; set; }
            public string ExamName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public string SemesterName { get; set; } = string.Empty;
            public int ExaminersCount { get; set; }
            public int SubmissionsCount { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Exams = await _apiClient.GetAsync<List<ExamItem>>("/api/manager/exams");
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