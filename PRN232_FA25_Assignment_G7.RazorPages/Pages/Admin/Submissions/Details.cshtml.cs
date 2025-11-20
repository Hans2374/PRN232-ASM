using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Submissions
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public DetailsModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public SubmissionDetail Submission { get; set; } = default!;

        public class SubmissionDetail
        {
            public int Id { get; set; }
            public string StudentName { get; set; } = string.Empty;
            public string ExamName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public DateTime SubmittedAt { get; set; }
            public string FilePath { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string ReviewStatus { get; set; } = string.Empty;
            public decimal? Score { get; set; }
            public string? GradingComments { get; set; }
            public string? GradedBy { get; set; }
            public DateTime? GradedAt { get; set; }
            public decimal? SecondScore { get; set; }
            public string? SecondGradingComments { get; set; }
            public string? SecondGradedBy { get; set; }
            public DateTime? SecondGradedAt { get; set; }
            public decimal? FinalScore { get; set; }
            public string? ModeratorComments { get; set; }
            public string? ModeratorAdjustedBy { get; set; }
            public DateTime? ModeratorAdjustedAt { get; set; }
            public List<ViolationItem> Violations { get; set; } = new();
        }

        public class ViolationItem
        {
            public int Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public string ReviewStatus { get; set; } = string.Empty;
            public string? ReviewedBy { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var submission = await _apiClient.GetAsync<SubmissionDetail>($"/api/submissions/{id}/detail");
                if (submission == null)
                {
                    TempData["Error"] = "Submission not found";
                    return RedirectToPage("./Index");
                }

                Submission = submission;
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading submission: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }
    }
}
