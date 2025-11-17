using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApiClient _apiClient;

    public CreateModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CreateUserRequest UserRequest { get; set; } = new();

    public void OnGet()
    {
        // Initialize page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _apiClient.CreateUserAsync(UserRequest);

            if (result != null)
            {
                TempData["Success"] = $"User '{result.Username}' created successfully.";
                return RedirectToPage("/Admin/Users/Index");
            }
            else
            {
                TempData["Error"] = "Failed to create user.";
                return Page();
            }
        }
        catch (HttpRequestException ex)
        {
            // Parse error message from API
            if (ex.Message.Contains("already taken"))
            {
                ModelState.AddModelError("UserRequest.Username", "This username is already taken.");
            }
            else
            {
                TempData["Error"] = "Failed to create user. Please check your input.";
            }
            return Page();
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while creating the user.";
            return Page();
        }
    }
}
