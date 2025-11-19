using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Submissions
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public IndexModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public List<SubmissionItem> Submissions { get; set; } = new();

        public class SubmissionItem
        {
            public int Id { get; set; }
            public string StudentName { get; set; } = string.Empty;
            public string ExamName { get; set; } = string.Empty;
            public DateTime SubmittedAt { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal? Score { get; set; }
            public decimal? SecondScore { get; set; }
            public decimal? FinalScore { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var submissions = await _apiClient.GetAsync<List<SubmissionItem>>("/api/submissions");
                Submissions = submissions ?? new List<SubmissionItem>();
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading submissions: {ex.Message}";
                return Page();
            }
        }
    }
}
