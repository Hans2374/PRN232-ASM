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
            public Guid Id { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public ExamItem? Exam { get; set; }
            public DateTime CreatedAt { get; set; }
            public string SubmissionStatus { get; set; } = string.Empty;
            public decimal? Score { get; set; }
            public decimal? SecondScore { get; set; }
            public int ViolationCount { get; set; }
        }

        public class ExamItem
        {
            public string Name { get; set; } = string.Empty;
        }

        public class ODataResponse<T>
        {
            public List<T> Value { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<ODataResponse<SubmissionItem>>("/odata/Submissions?$expand=Exam&$orderby=CreatedAt desc&$top=50");
                Submissions = response?.Value ?? new List<SubmissionItem>();
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
