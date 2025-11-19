using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Violations
{
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public DetailsModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public ViolationDetails? Violation { get; set; }

        [BindProperty]
        public string? Action { get; set; }

        [BindProperty]
        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string? Comments { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Violation = await _apiClient.GetAsync<ViolationDetails>($"/api/manager/violations/{id}");
                if (Violation == null)
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
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                TempData["AuthError"] = "You do not have permission to access this page.";
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return await OnGetAsync(id);
            }

            try
            {
                var request = new { Action, Comments };
                await _apiClient.PostAsync<object, object>($"/api/manager/violations/{id}/decision", request);
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

    public class ViolationDetails
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string? ExaminerName { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? Evidence { get; set; }
        public List<ViolationTimelineItem>? Timeline { get; set; }
    }

    public class ViolationTimelineItem
    {
        public DateTime Timestamp { get; set; }
        public string? Action { get; set; }
        public string? Actor { get; set; }
        public string? Details { get; set; }
    }
}