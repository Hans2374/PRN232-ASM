using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Manager.Exams
{
    [Authorize]
    public class AssignModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public AssignModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        public Guid ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public List<SelectListItem> AvailableExaminers { get; set; } = new();

        [BindProperty]
        public AssignInput Input { get; set; } = new();

        public class AssignInput
        {
            public Guid ExaminerId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid examId)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            ExamId = examId;

            try
            {
                // Get exam details
                var exam = await _apiClient.GetAsync<ExamDetails>($"/api/manager/exams/{examId}");
                ExamName = exam?.ExamName ?? "Unknown Exam";

                // Get available examiners
                var examiners = await _apiClient.GetAsync<List<ExaminerItem>>($"/api/manager/exams/{examId}/unassigned-examiners");
                AvailableExaminers = examiners?.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })?.ToList() ?? new List<SelectListItem>();

                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach server.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid examId)
        {
            if (!_authSession.IsInRole(HttpContext, "Manager"))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(examId);
                return Page();
            }

            try
            {
                var request = new { examinerId = Input.ExaminerId };
                await _apiClient.PostAsync<object, object>($"/api/manager/exams/{examId}/assign", request);
                TempData["SuccessMessage"] = "Examiner assigned successfully.";
                return RedirectToPage("/Manager/Exams/Details", new { id = examId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Failed to assign examiner.";
                return Page();
            }
        }

        public class ExamDetails
        {
            public Guid Id { get; set; }
            public string ExamName { get; set; } = string.Empty;
        }

        public class ExaminerItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
