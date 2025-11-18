/*
 * USER MANAGEMENT - LIST PAGE
 * 
 * API Configuration:
 * - Ensure appsettings.json contains: "ApiSettings": { "BaseUrl": "https://localhost:53776" }
 * - Change port 53776 to match your API's HTTPS port
 * - API Endpoint: GET /api/admin/users
 * 
 * Authorization:
 * - Only Admin role can access this page
 * - Non-admin users will be redirected to /Account/Login
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApiClient apiClient, ILogger<IndexModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public List<UserResponse> Users { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading users list. SearchTerm: {SearchTerm}", SearchTerm ?? "None");
            
            var users = await _apiClient.GetUsersAsync();
            Users = users ?? new List<UserResponse>();

            // Client-side filtering if search term provided
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Users = Users.Where(u =>
                    u.Username.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Role.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            _logger.LogInformation("Loaded {Count} users", Users.Count);
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to users list");
            TempData["Error"] = "Your session has expired. Please login again.";
            return RedirectToPage("/Account/Login");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error while loading users. API may be unreachable.");
            TempData["Error"] = "Failed to load users. Please check that the API is running and accessible.";
            Users = new List<UserResponse>();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading users");
            TempData["Error"] = "An unexpected error occurred. Please try again or check the logs.";
            Users = new List<UserResponse>();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Attempting to deactivate user {UserId}", id);
            
            var success = await _apiClient.DeleteUserAsync(id);
            
            if (success)
            {
                TempData["Success"] = "User deactivated successfully.";
                _logger.LogInformation("User {UserId} deactivated successfully", id);
            }
            else
            {
                TempData["Error"] = "Failed to deactivate user. Please try again.";
                _logger.LogWarning("Delete operation returned false for user {UserId}", id);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to delete user {UserId}", id);
            TempData["Error"] = "Your session has expired. Please login again.";
            return RedirectToPage("/Account/Login");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error while deleting user {UserId}", id);
            TempData["Error"] = "Failed to connect to API. Please ensure the API is running.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting user {UserId}", id);
            TempData["Error"] = "An unexpected error occurred while deleting the user.";
        }

        return RedirectToPage();
    }
}
