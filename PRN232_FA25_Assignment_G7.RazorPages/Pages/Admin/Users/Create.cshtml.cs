/*
 * USER MANAGEMENT - CREATE PAGE
 * 
 * API Configuration:
 * - API Endpoint: POST /api/admin/users
 * - Accepts CreateUserRequest and returns UserResponse
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
    public CreateUserRequest UserInput { get; set; } = new();

    public List<SelectListItem> AvailableRoles { get; set; } = new();

    public void OnGet()
    {
        _logger.LogInformation("Initializing Create User page");
        LoadRoles();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadRoles();
            return Page();
        }

        try
        {
            _logger.LogInformation("Attempting to create user: {Username}", UserInput.Username);
            
            var result = await _apiClient.CreateUserAsync(UserInput);

            if (result != null)
            {
                TempData["Success"] = $"User '{result.Username}' created successfully.";
                _logger.LogInformation("User {Username} created successfully with ID {UserId}", result.Username, result.Id);
                return RedirectToPage("/Admin/Users/Index");
            }
            else
            {
                TempData["Error"] = "Failed to create user. No response from API.";
                LoadRoles();
                return Page();
            }
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409") || ex.Message.Contains("Conflict"))
        {
            _logger.LogWarning("Username {Username} already exists", UserInput.Username);
            ModelState.AddModelError("UserInput.Username", "This username is already taken. Please choose a different username.");
            LoadRoles();
            return Page();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400") || ex.Message.Contains("BadRequest"))
        {
            _logger.LogWarning(ex, "Bad request while creating user");
            TempData["Error"] = "Invalid user data. Please check all fields and try again.";
            LoadRoles();
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to create user");
            TempData["Error"] = "Your session has expired. Please login again.";
            return RedirectToPage("/Account/Login");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error while creating user");
            TempData["Error"] = "Failed to connect to API. Please ensure the API is running.";
            LoadRoles();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating user");
            TempData["Error"] = "An unexpected error occurred. Please try again or contact support.";
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
