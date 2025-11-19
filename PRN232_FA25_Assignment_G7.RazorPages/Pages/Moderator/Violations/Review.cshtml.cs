using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Moderator.Violations
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

        public ViolationReviewDetails? ViolationDetails { get; set; }

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
                ViolationDetails = await _apiClient.GetAsync<ViolationReviewDetails>($"/api/moderator/violations/{id}");
                if (ViolationDetails == null)
                {
                    TempData["Error"] = "Violation not found.";
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
                TempData["Error"] = $"Failed to load violation details: {ex.Message}";
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

            if ((Decision == "Reject" || Decision == "Escalate") && string.IsNullOrWhiteSpace(Comments))
            {
                ModelState.AddModelError("Comments", "Comments are required when rejecting or escalating a violation.");
                return await OnGetAsync(id);
            }

            try
            {
                var request = new ViolationDecisionRequest
                {
                    Decision = Decision!,
                    Comments = Comments
                };

                await _apiClient.PostAsync<ViolationDecisionRequest, object>($"/api/moderator/violations/{id}/decision", request);
                TempData["SuccessMessage"] = "Violation decision submitted successfully.";
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

    public class ViolationReviewDetails
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? ExamName { get; set; }
        public string? ExaminerName { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Evidence { get; set; }
        public string? Status { get; set; }
        public DateTime ReportedAt { get; set; }
        public List<ViolationTimelineItem>? Timeline { get; set; }
    }

    public class ViolationTimelineItem
    {
        public DateTime Timestamp { get; set; }
        public string? Action { get; set; }
        public string? Actor { get; set; }
        public string? Details { get; set; }
    }

    public class ViolationDecisionRequest
    {
        public string? Decision { get; set; }
        public string? Comments { get; set; }
    }
}