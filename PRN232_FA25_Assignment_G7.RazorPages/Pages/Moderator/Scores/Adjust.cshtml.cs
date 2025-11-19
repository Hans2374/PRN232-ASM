using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Scores
{
    [Authorize]
    public class AdjustModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public AdjustModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public AdjustInput Input { get; set; } = default!;

        public class AdjustInput
        {
            public int SubmissionId { get; set; }
            public decimal FinalScore { get; set; }
            public string ModeratorComments { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var request = new
            {
                finalScore = Input.FinalScore,
                moderatorComments = Input.ModeratorComments
            };
            await _apiClient.PostAsync<object, object>($"/api/submissions/{Input.SubmissionId}/moderator-adjust-score", request);
            TempData["Success"] = "Score adjusted successfully.";
            return RedirectToPage("/Moderator/Dashboard");
        }
    }
}
