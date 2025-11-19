using System;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Submissions
{
    [Authorize(Roles = "Examiner")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public IndexModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Guid? ExamId { get; set; }

        public List<SubmissionItem>? Data { get; set; }

        public class SubmissionItem
        {
            public Guid Id { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public DateTime SubmittedAt { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(Guid? examId)
        {
            ExamId = examId;
            if (examId == null)
            {
                TempData["Error"] = "ExamId is required to view submissions.";
                return Page();
            }

            try
            {
                Data = await _apiClient.GetAsync<List<SubmissionItem>>($"/api/examiner/exams/{examId}/submissions");
                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["AuthError"] = ex.Message;
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach API";
                return Page();
            }
        }
    }
}