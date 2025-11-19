using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Submissions
{
    [Authorize(Roles = "Examiner")]
    public class DoubleGradeModel : PageModel
    {
        private readonly ApiClient _apiClient;
        private readonly AuthSession _authSession;

        public DoubleGradeModel(ApiClient apiClient, AuthSession authSession)
        {
            _apiClient = apiClient;
            _authSession = authSession;
        }

        [BindProperty]
        public DoubleGradeInput Input { get; set; } = new();

        public SubmissionDetailDto? Submission { get; set; }
        public List<RubricItemDto>? Rubric { get; set; }
        public Dictionary<int, int>? FirstExaminerScores { get; set; } // ItemId -> Score

        public class DoubleGradeInput
        {
            public List<GradeItem> Scores { get; set; } = new();
            public string? Comments { get; set; }
        }

        public class GradeItem
        {
            public int ItemId { get; set; }
            public int Score { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!_authSession.IsExaminer(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                Submission = await _apiClient.GetAsync<SubmissionDetailDto>($"/api/submissions/{id}");
                if (Submission == null)
                {
                    TempData["Error"] = "Submission not found.";
                    return Page();
                }

                // Assume examId is available in Submission or from route
                // For simplicity, assume we get rubric by examId, but since it's not in DTO, we might need to adjust
                // Let's assume API provides rubric in submission or we call /api/rubrics/{examId}
                // For now, placeholder
                Rubric = await _apiClient.GetAsync<List<RubricItemDto>>($"/api/rubrics/{Submission.ExamId}");

                // Get first examiner scores - assume API provides this
                FirstExaminerScores = await _apiClient.GetAsync<Dictionary<int, int>>($"/api/submissions/{id}/first-scores");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Cannot reach the server.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (!_authSession.IsExaminer(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var request = new
                {
                    scores = Input.Scores,
                    comments = Input.Comments
                };
                await _apiClient.PostAsync<object, object>($"/api/submissions/{id}/double-grade", request);
                TempData["SuccessMessage"] = "Double grading completed.";
                return RedirectToPage("/Examiner/Submissions/Index");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["AuthError"] = "Access denied.";
                return Page();
            }
            catch (ApiException)
            {
                TempData["Error"] = "Failed to submit double grade.";
                return Page();
            }
        }
    }

    public class RubricItemDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public int MaxScore { get; set; }
    }
}