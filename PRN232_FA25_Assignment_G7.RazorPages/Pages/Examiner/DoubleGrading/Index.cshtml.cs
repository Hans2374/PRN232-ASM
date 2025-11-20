using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.DoubleGrading;

[Authorize(Roles = "Examiner")]
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApiClient apiClient, AuthSession authSession, ILogger<IndexModel> logger)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _logger = logger;
    }

    public PagedResult<DoubleGradingTaskDto>? Tasks { get; set; }
    public int PageNumber { get; set; } = 1;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        if (!_authSession.IsInRole(HttpContext, "Examiner"))
        {
            return RedirectToPage("/Account/Login");
        }

        PageNumber = page;

        try
        {
            var filter = new DoubleGradingFilter
            {
                PageNumber = page,
                PageSize = 25
            };

            Tasks = await _apiClient.GetDoubleGradingTasksAsync(filter);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized access to double grading tasks");
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading double grading tasks");
            ErrorMessage = "Unable to load double grading tasks. Please try again later.";
            return Page();
        }
    }
}