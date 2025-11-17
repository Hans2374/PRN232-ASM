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

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<UserResponse> Users { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Users = await _apiClient.GetUsersAsync();
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            // Preserve intent to return to this page after login
            TempData["AuthError"] = ex.Message;
            return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Admin/Users/Index") });
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"Failed to load users. Error: {ex.Message}";
            return Page();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An unexpected error occurred: {ex.Message}";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var success = await _apiClient.DeleteUserAsync(id);

            if (success)
            {
                TempData["Success"] = "User deactivated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to deactivate user.";
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while deactivating the user.";
        }

        return RedirectToPage();
    }
}
