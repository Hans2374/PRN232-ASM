using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Rubrics
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public IndexModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public List<RubricItem> Rubrics { get; set; } = new();

        public class RubricItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int MaxScore { get; set; }
            public int ExamId { get; set; }
            public string ExamName { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var rubrics = await _apiClient.GetAsync<List<RubricItem>>("/api/rubrics");
                Rubrics = rubrics ?? new List<RubricItem>();
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading rubrics: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _apiClient.DeleteAsync($"/api/rubrics/{id}");
                TempData["Success"] = "Rubric deleted successfully";
                return RedirectToPage();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex)
            {
                TempData["Error"] = $"Failed to delete rubric: {ex.Message}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting rubric: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
