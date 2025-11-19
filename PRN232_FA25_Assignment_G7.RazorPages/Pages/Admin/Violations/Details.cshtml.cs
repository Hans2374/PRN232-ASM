using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Violations
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public DetailsModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public ViolationDetail Violation { get; set; } = default!;

        public class ViolationDetail
        {
            public int Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public int SubmissionId { get; set; }
            public string StudentName { get; set; } = string.Empty;
            public string ExamName { get; set; } = string.Empty;
            public string ReviewStatus { get; set; } = string.Empty;
            public string? ReviewedBy { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? ReviewComments { get; set; }
            public bool IsZeroScore { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var violation = await _apiClient.GetAsync<ViolationDetail>($"/api/violations/{id}");
                if (violation == null)
                {
                    TempData["Error"] = "Violation not found";
                    return RedirectToPage("./Index");
                }

                Violation = violation;
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading violation: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }
    }
}
