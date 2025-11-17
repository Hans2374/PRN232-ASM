using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ApiClient _apiClient;

    public EditModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public UpdateUserRequest UpdateRequest { get; set; } = new();

    public new UserResponse? User { get; set; }

    public string CurrentUsername { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            User = await _apiClient.GetUserByIdAsync(id);

            if (User == null)
            {
                TempData["Error"] = "User not found.";
                return Page();
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

            return Page();
        }
        catch (Exception)
        {
            TempData["Error"] = "Failed to load user details.";
            return RedirectToPage("/Admin/Users/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            // Reload user data for display
            User = await _apiClient.GetUserByIdAsync(id);
            CurrentUsername = User?.Username ?? string.Empty;
            return Page();
        }

        try
        {
            var result = await _apiClient.UpdateUserAsync(id, UpdateRequest);

            if (result != null)
            {
                TempData["Success"] = $"User '{result.Username}' updated successfully.";
                return RedirectToPage("/Admin/Users/Index");
            }
            else
            {
                TempData["Error"] = "Failed to update user.";
                User = await _apiClient.GetUserByIdAsync(id);
                CurrentUsername = User?.Username ?? string.Empty;
                return Page();
            }
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Failed to update user. Please check your input.";
            User = await _apiClient.GetUserByIdAsync(id);
            CurrentUsername = User?.Username ?? string.Empty;
            return Page();
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while updating the user.";
            User = await _apiClient.GetUserByIdAsync(id);
            CurrentUsername = User?.Username ?? string.Empty;
            return Page();
        }
    }
}
