using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Semesters;

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
    public CreateSemesterRequest SemesterInput { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (SemesterInput.EndDate <= SemesterInput.StartDate)
        {
            ModelState.AddModelError("SemesterInput.EndDate", "End date must be after start date.");
            return Page();
        }

        try
        {
            var result = await _apiClient.PostAsync<CreateSemesterRequest, SemesterResponse>("/api/semesters", SemesterInput);
            
            if (result != null)
            {
                TempData["Success"] = "Semester created successfully.";
                return RedirectToPage("Index");
            }
            else
            {
                TempData["Error"] = "Failed to create semester.";
                return Page();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creating semester");
            TempData["Error"] = $"Error creating semester: {ex.Message}";
            return Page();
        }
    }
}
