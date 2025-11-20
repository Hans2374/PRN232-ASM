using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Subjects;

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
    public UpdateSubjectRequest SubjectInput { get; set; } = new();
    
    public Guid SubjectId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            var subject = await _apiClient.GetAsync<SubjectResponse>($"/api/subjects/{id}");
            
            if (subject == null)
            {
                TempData["Error"] = "Subject not found.";
                return RedirectToPage("Index");
            }

            SubjectId = subject.Id;
            SubjectInput = new UpdateSubjectRequest
            {
                Code = subject.Code,
                Name = subject.Name
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading subject {Id}", id);
            TempData["Error"] = "Failed to load subject.";
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            SubjectId = id;
            return Page();
        }

        try
        {
            var result = await _apiClient.PutAsync<UpdateSubjectRequest, SubjectResponse>($"/api/subjects/{id}", SubjectInput);
            
            if (result != null)
            {
                TempData["Success"] = "Subject updated successfully.";
                return RedirectToPage("Index");
            }
            else
            {
                TempData["Error"] = "Failed to update subject.";
                SubjectId = id;
                return Page();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error updating subject {Id}", id);
            TempData["Error"] = $"Error updating subject: {ex.Message}";
            SubjectId = id;
            return Page();
        }
    }
}
