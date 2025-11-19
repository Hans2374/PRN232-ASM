using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Violations
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public DetailsModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public ViolationDetail? Violation { get; set; }

        [BindProperty]
        public string ReviewComments { get; set; } = string.Empty;

        public class ViolationDetail
        {
            public int Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string ExamName { get; set; } = string.Empty;
            public string ReviewStatus { get; set; } = string.Empty;
            public string? ReviewedBy { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? ReviewComments { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Violation = await _apiClient.GetAsync<ViolationDetail>($"/api/violations/{id}");
            if (Violation == null)
            {
                TempData["Error"] = "Violation not found.";
                return RedirectToPage("./Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var request = new { reviewComments = ReviewComments };
            await _apiClient.PostAsync<object, object>($"/api/violations/{id}/approve-moderator", request);
            TempData["Success"] = "Violation approved.";
            return RedirectToPage("./Index");
        }
    }
}
