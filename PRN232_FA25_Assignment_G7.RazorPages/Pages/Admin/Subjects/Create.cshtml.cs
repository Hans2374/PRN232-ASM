using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Subjects;

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
    public CreateSubjectRequest SubjectInput { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _apiClient.PostAsync<CreateSubjectRequest, SubjectResponse>("/api/subjects", SubjectInput);
            
            if (result != null)
            {
                TempData["Success"] = "Subject created successfully.";
                return RedirectToPage("Index");
            }
            else
            {
                TempData["Error"] = "Failed to create subject.";
                return Page();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creating subject");
            
            if (ex.Message.Contains("already exists"))
            {
                ModelState.AddModelError("SubjectInput.Code", "A subject with this code already exists.");
            }
            else
            {
                TempData["Error"] = $"Error creating subject: {ex.Message}";
            }
            
            return Page();
        }
    }
}
