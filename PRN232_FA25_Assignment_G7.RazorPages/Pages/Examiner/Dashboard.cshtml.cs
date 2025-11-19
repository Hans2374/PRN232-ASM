using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner
{
    [Authorize(Roles = "Examiner")]
    public class DashboardModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public DashboardModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }


        public ExaminerDashboardResponse? Data { get; set; }

        public class ExaminerDashboardResponse
        {
            public int AssignedExamsCount { get; set; }
            public int PendingSubmissionsCount { get; set; }
            public int GradedSubmissionsCount { get; set; }
            public List<AssignedExamSummary> AssignedExams { get; set; } = new();
        }

        public class AssignedExamSummary
        {
            public Guid ExamId { get; set; }
            public string ExamName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public DateTime ExamDate { get; set; }
            public int TotalSubmissions { get; set; }
            public int GradedSubmissions { get; set; }
            public bool IsPrimaryGrader { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Data = await _apiClient.GetAsync<ExaminerDashboardResponse>("/api/examiner/dashboard");
                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                // API returned 401 or 403 (handled by ApiClient as UnauthorizedAccessException)
                TempData["AuthError"] = ex.Message;
                return Page();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach API";
                return Page();
            }
        }
    }
}
