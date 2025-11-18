using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Exams;

[Authorize(Policy = "AdminOrManager")]
public class CreateModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ApiClient apiClient, ILogger<CreateModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [BindProperty]
    public CreateExamRequest ExamInput { get; set; } = new();

    public List<SelectListItem> Subjects { get; set; } = new();
    public List<SelectListItem> Semesters { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            var result = await _apiClient.PostAsync<CreateExamRequest, ExamResponse>("/api/exams", ExamInput);
            
            if (result != null)
            {
                TempData["Success"] = "Exam created successfully.";
                return RedirectToPage("Index");
            }
            else
            {
                TempData["Error"] = "Failed to create exam.";
                await LoadDropdownsAsync();
                return Page();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creating exam");
            TempData["Error"] = $"Error creating exam: {ex.Message}";
            await LoadDropdownsAsync();
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        try
        {
            var subjects = await _apiClient.GetAsync<List<SubjectResponse>>("/api/subjects");
            Subjects = subjects?.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Code} - {s.Name}"
            }).ToList() ?? new List<SelectListItem>();

            var semesters = await _apiClient.GetAsync<List<SemesterResponse>>("/api/semesters");
            Semesters = semesters?.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList() ?? new List<SelectListItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dropdown data");
        }
    }
}
