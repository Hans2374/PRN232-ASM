using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Violations
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public HistoryModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public List<ViolationItem>? Data { get; set; }

        public class ViolationItem
        {
            public int Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string ExamName { get; set; } = string.Empty;
            public string ReviewStatus { get; set; } = string.Empty;
            public string? ReviewedBy { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Data = await _apiClient.GetAsync<List<ViolationItem>>("/api/moderator/violations/history");
                return Page();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach API";
                return Page();
            }
        }
    }
}
