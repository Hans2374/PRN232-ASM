using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Models;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        ApiClient apiClient, 
        AuthSession authSession,
        ILogger<LoginModel> logger)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _logger = logger;
    }

    [BindProperty]
    public LoginRequest LoginRequest { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Do not clear session/token on GET. Clearing here causes
        // valid tokens to be wiped when redirected to login.
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please enter both username and password.";
            return Page();
        }

        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", LoginRequest.Username);

            // Test API connectivity first
            var isReachable = await _apiClient.IsApiReachableAsync();
            if (!isReachable)
            {
                _logger.LogError("API is not reachable");
                ErrorMessage = "Cannot connect to the API server. Please ensure the API is running and try again.";
                return Page();
            }

            // Call API to authenticate
            var response = await _apiClient.LoginAsync(LoginRequest);

            if (response == null || string.IsNullOrEmpty(response.Token))
            {
                _logger.LogWarning("Login failed for user: {Username} - No token received", LoginRequest.Username);
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            _logger.LogInformation("Login successful for user: {Username}", LoginRequest.Username);

            // Save JWT token using AuthSession
            _authSession.SaveToken(response.Token);

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new Claim(ClaimTypes.Name, response.Username),
                new Claim(ClaimTypes.Email, response.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, response.Role),
                new Claim("FullName", response.FullName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Ensure session is committed before redirect
            await HttpContext.Session.CommitAsync();

            // Clean any previous error notices to avoid showing both messages
            TempData.Remove("Error");
            TempData.Remove("AuthError");
            TempData["Success"] = $"Welcome back, {response.FullName}!";

            _logger.LogInformation("User {Username} authenticated successfully with role {Role}", 
                response.Username, response.Role);

            // Verify token was saved
            var savedToken = _authSession.GetToken();
            _logger.LogInformation("Token saved verification: {TokenExists}", !string.IsNullOrEmpty(savedToken));

            // Support returnUrl if provided
            var returnUrl = Request.Query["returnUrl"].ToString();
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            // Redirect to role-specific dashboard
            switch (response.Role)
            {
                case "Admin":
                    return RedirectToPage("/Admin/Dashboard");
                case "Manager":
                    return RedirectToPage("/Manager/Dashboard");
                case "Moderator":
                    return RedirectToPage("/Moderator/Dashboard");
                case "Examiner":
                    return RedirectToPage("/Examiner/Dashboard");
                default:
                    return RedirectToPage("/Index"); // fallback
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Cannot connect to API"))
        {
            _logger.LogError(ex, "Connection error for user: {Username}", LoginRequest.Username);
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized login attempt for user: {Username}", LoginRequest.Username);
            ErrorMessage = "Invalid username or password.";
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "API communication error for user: {Username}", LoginRequest.Username);
            ErrorMessage = $"API Error: {ex.Message}";
            return Page();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during login for user: {Username}", LoginRequest.Username);
            ErrorMessage = "Unable to connect to the authentication server. Please ensure the API is running.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user: {Username}", LoginRequest.Username);
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            return Page();
        }
    }
}
