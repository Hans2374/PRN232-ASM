using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Rubrics
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public CreateModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public RubricInput Input { get; set; } = default!;

        public List<SelectListItem> Exams { get; set; } = new();

        public class RubricInput
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int MaxScore { get; set; }
            public int ExamId { get; set; }
        }

        public class ExamItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadExamsAsync();
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading form: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadExamsAsync();
                return Page();
            }

            try
            {
                var request = new
                {
                    name = Input.Name,
                    description = Input.Description,
                    maxScore = Input.MaxScore,
                    examId = Input.ExamId
                };

                await _apiClient.PostAsync<object, object>("/api/rubrics", request);

                TempData["Success"] = "Rubric created successfully";
                return RedirectToPage("./Index");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadExamsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating rubric: {ex.Message}";
                await LoadExamsAsync();
                return Page();
            }
        }

        private async Task LoadExamsAsync()
        {
            var exams = await _apiClient.GetAsync<List<ExamItem>>("/api/exams");
            Exams = exams?.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Name
            }).ToList() ?? new List<SelectListItem>();
        }
    }
}
