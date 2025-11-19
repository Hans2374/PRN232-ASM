using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Scores
{
    public class ReviewModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public ReviewModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public ScoreReviewDetails? ScoreDetails { get; set; }

        [BindProperty]
        public string? Decision { get; set; }

        [BindProperty]
        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string? Comments { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                ScoreDetails = await _apiClient.GetAsync<ScoreReviewDetails>($"/api/moderator/scores/{id}");
                if (ScoreDetails == null)
                {
                    TempData["Error"] = "Score not found.";
                    return RedirectToPage("./Index");
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load score details: {ex.Message}";
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!_authSession.IsInRole(HttpContext, "Moderator"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return await OnGetAsync(id);
            }

            if (Decision == "Reject" && string.IsNullOrWhiteSpace(Comments))
            {
                ModelState.AddModelError("Comments", "Comments are required when rejecting a score.");
                return await OnGetAsync(id);
            }

            try
            {
                var request = new ScoreDecisionRequest
                {
                    Decision = Decision!,
                    Comments = Comments
                };

                await _apiClient.PostAsync<ScoreDecisionRequest, object>($"/api/moderator/scores/{id}/decision", request);
                TempData["SuccessMessage"] = "Score decision submitted successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Your session has expired. Please log in again.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to submit decision: {ex.Message}";
                return await OnGetAsync(id);
            }

            return RedirectToPage("./Index");
        }
    }

    public class ScoreReviewDetails
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? ExamName { get; set; }
        public decimal ExaminerScore { get; set; }
        public decimal? AutoScore { get; set; }
        public string? ExaminerComments { get; set; }
        public string? Discrepancies { get; set; }
        public string? SubmissionFileUrl { get; set; }
        public string? Status { get; set; }
    }

    public class ScoreDecisionRequest
    {
        public string? Decision { get; set; }
        public string? Comments { get; set; }
    }
}