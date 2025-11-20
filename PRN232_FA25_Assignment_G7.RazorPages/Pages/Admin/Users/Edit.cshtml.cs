/*
 * USER MANAGEMENT - EDIT PAGE
 * 
 * API Configuration:
 * - API Endpoints: 
 *   - GET /api/admin/users/{id} - Retrieve user details
 *   - PUT /api/admin/users/{id} - Update user
 * 
 * Authorization:
 * - Only Admin role can access this page
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
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
    public UpdateUserRequest UpdateRequest { get; set; } = new();

    public new UserResponse? User { get; set; }

    public string CurrentUsername { get; set; } = string.Empty;

    public List<SelectListItem> AvailableRoles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Loading user details for edit. UserId: {UserId}", id);
            
            User = await _apiClient.GetUserByIdAsync(id);

            if (User == null)
            {
                _logger.LogWarning("User not found. UserId: {UserId}", id);
                TempData["Error"] = $"User with ID {id} not found.";
                return RedirectToPage("/Admin/Users/Index");
            }

            CurrentUsername = User.Username;

            // Populate the form with current user data
            UpdateRequest = new UpdateUserRequest
            {
                FullName = User.FullName,
                Email = User.Email,
                Role = User.Role,
                IsActive = User.IsActive
            };

            LoadRoles();
            _logger.LogInformation("Loaded user {Username} for editing", User.Username);
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to edit user");
            TempData["Error"] = "Your session has expired. Please login again.";
            return RedirectToPage("/Account/Login");
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while loading user. UserId: {UserId} - {StatusCode}", id, ex.StatusCode);
            TempData["Error"] = "Failed to connect to API. Please ensure the API is running.";
            return RedirectToPage("/Admin/Users/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while loading user. UserId: {UserId}", id);
            TempData["Error"] = "An unexpected error occurred while loading user details.";
            return RedirectToPage("/Admin/Users/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            // Reload user data for display
            try
            {
                User = await _apiClient.GetUserByIdAsync(id);
                CurrentUsername = User?.Username ?? string.Empty;
                LoadRoles();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading user after validation failure");
            }
            return Page();
        }

        try
        {
            _logger.LogInformation("Attempting to update user. UserId: {UserId}", id);
            
            var result = await _apiClient.UpdateUserAsync(id, UpdateRequest);

            if (result != null)
            {
                TempData["Success"] = $"User '{result.Username}' updated successfully.";
                _logger.LogInformation("User {Username} updated successfully", result.Username);
                return RedirectToPage("/Admin/Users/Index");
            }
            else
            {
                TempData["Error"] = "Failed to update user. No response from API.";
                User = await _apiClient.GetUserByIdAsync(id);
                CurrentUsername = User?.Username ?? string.Empty;
                LoadRoles();
                return Page();
            }
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User not found during update. UserId: {UserId}", id);
            TempData["Error"] = "User not found. It may have been deleted.";
            return RedirectToPage("/Admin/Users/Index");
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogWarning(ex, "Bad request while updating user");
            TempData["Error"] = "Invalid user data. Please check all fields and try again.";
            User = await _apiClient.GetUserByIdAsync(id);
            CurrentUsername = User?.Username ?? string.Empty;
            LoadRoles();
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update user");
            TempData["Error"] = "Your session has expired. Please login again.";
            return RedirectToPage("/Account/Login");
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while updating user: {StatusCode}", ex.StatusCode);
            TempData["Error"] = "Failed to connect to API. Please ensure the API is running.";
            User = await _apiClient.GetUserByIdAsync(id);
            CurrentUsername = User?.Username ?? string.Empty;
            LoadRoles();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating user");
            TempData["Error"] = "An unexpected error occurred. Please try again or contact support.";
            User = await _apiClient.GetUserByIdAsync(id);
            CurrentUsername = User?.Username ?? string.Empty;
            LoadRoles();
            return Page();
        }
    }

    private void LoadRoles()
    {
        AvailableRoles = new List<SelectListItem>
        {
            new SelectListItem { Value = "Admin", Text = "Admin - Full system access" },
            new SelectListItem { Value = "Manager", Text = "Manager - Exam and subject management" },
            new SelectListItem { Value = "Moderator", Text = "Moderator - Review and approve submissions" },
            new SelectListItem { Value = "Examiner", Text = "Examiner - Grade submissions" }
        };
    }
}

