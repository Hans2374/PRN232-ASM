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
    public class ViewModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public ViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public SubmissionDetailDto? Submission { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Submission = await _apiClient.GetAsync<SubmissionDetailDto>($"/api/submissions/{id}/detail");
                if (Submission == null)
                {
                    ErrorMessage = "Submission not found.";
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
    }
}
