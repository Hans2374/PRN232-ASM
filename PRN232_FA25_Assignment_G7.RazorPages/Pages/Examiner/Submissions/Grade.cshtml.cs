using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Submissions
{
    [Authorize(Roles = "Examiner")]
    public class GradeModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public GradeModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public GradeInput Input { get; set; } = new();

        public SubmissionDetailDto? Submission { get; set; }

        public class GradeInput
        {
            public int Score { get; set; }
            public string? Comments { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Submission = await _apiClient.GetAsync<SubmissionDetailDto>($"/api/submissions/{id}/detail");
                if (Submission == null)
                {
                    TempData["Error"] = "Submission not found";
                }
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = ex.Content ?? ex.Message;
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach API";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var request = new { score = Input.Score, comments = Input.Comments };
                await _apiClient.PostAsync<object, object>($"/api/submissions/{id}/grade", request);
                TempData["SuccessMessage"] = "Grade submitted";
                return RedirectToPage("/Examiner/Submissions/View2", new { id });
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = ex.Content ?? ex.Message;
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Failed to submit grade";
                return Page();
            }
        }
    }

    // Reuse DTO (minimal) - duplicate here to avoid dependencies on corrupted files
    public class SubmissionDetailDto
    {
        public Guid Id { get; set; }
        public string? StudentName { get; set; }
        public string? FileUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int? Score { get; set; }
        public string? Comments { get; set; }
        public Guid ExamId { get; set; }
    }
}

