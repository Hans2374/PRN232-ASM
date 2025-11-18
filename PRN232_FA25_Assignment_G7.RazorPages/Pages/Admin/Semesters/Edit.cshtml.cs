using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Semesters;

[Authorize(Policy = "AdminOrManager")]
public class EditModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<EditModel> _logger;

    public EditModel(ApiClient apiClient, ILogger<EditModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [BindProperty]
    public CreateSemesterRequest SemesterInput { get; set; } = new();
    
    public Guid SemesterId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            var semester = await _apiClient.GetAsync<SemesterResponse>($"/api/semesters/{id}");
            
            if (semester == null)
            {
                TempData["Error"] = "Semester not found.";
                return RedirectToPage("Index");
            }

            SemesterId = semester.Id;
            SemesterInput = new CreateSemesterRequest
            {
                Name = semester.Name,
                StartDate = semester.StartDate,
                EndDate = semester.EndDate
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading semester {Id}", id);
            TempData["Error"] = "Failed to load semester.";
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            SemesterId = id;
            return Page();
        }

        if (SemesterInput.EndDate <= SemesterInput.StartDate)
        {
            ModelState.AddModelError("SemesterInput.EndDate", "End date must be after start date.");
            SemesterId = id;
            return Page();
        }

        try
        {
            // Note: API doesn't have PUT for semesters in the controller, but we'll call it anyway
            // If the API returns 404/405, we'll handle it gracefully
            var result = await _apiClient.PutAsync<CreateSemesterRequest, SemesterResponse>($"/api/semesters/{id}", SemesterInput);
            
            if (result != null)
            {
                TempData["Success"] = "Semester updated successfully.";
                return RedirectToPage("Index");
            }
            else
            {
                TempData["Error"] = "Failed to update semester.";
                SemesterId = id;
                return Page();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error updating semester {Id}", id);
            
            if (ex.Message.Contains("404") || ex.Message.Contains("405"))
            {
                TempData["Warning"] = "Semester update endpoint not yet implemented in API. This is a placeholder page.";
            }
            else
            {
                TempData["Error"] = $"Error updating semester: {ex.Message}";
            }
            
            SemesterId = id;
            return Page();
        }
    }
}
