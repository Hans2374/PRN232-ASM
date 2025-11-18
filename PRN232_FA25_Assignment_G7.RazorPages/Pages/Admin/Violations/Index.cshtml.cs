using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Violations;

[Authorize(Policy = "AdminOrManager")]
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApiClient apiClient, ILogger<IndexModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public List<ViolationResponse> Violations { get; set; } = new();
    public Guid? FilterBySubmissionId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? submissionId)
    {
        try
        {
            FilterBySubmissionId = submissionId;
            
            if (submissionId.HasValue)
            {
                var response = await _apiClient.GetAsync<List<ViolationResponse>>($"/api/violations/submission/{submissionId.Value}");
                Violations = response ?? new List<ViolationResponse>();
            }
            else
            {
                // Note: API doesn't have a "get all violations" endpoint, so this is a placeholder
                TempData["Info"] = "Please select a submission to view its violations.";
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching violations");
            TempData["Error"] = "Failed to load violations. Please try again.";
            return Page();
        }
    }
}
