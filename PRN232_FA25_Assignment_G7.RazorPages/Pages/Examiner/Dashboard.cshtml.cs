using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner;

[Authorize(Roles = "Examiner")]
public class DashboardModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(ApiClient apiClient, AuthSession authSession, ILogger<DashboardModel> logger)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _logger = logger;
    }

    public ExaminerDashboardResponse? Dashboard { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_authSession.IsInRole(HttpContext, "Examiner"))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            Dashboard = await _apiClient.GetExaminerDashboardAsync();
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized access to examiner dashboard");
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading examiner dashboard");
            ErrorMessage = "Unable to load dashboard data. Please try again later.";
            return Page();
        }
    }
}
