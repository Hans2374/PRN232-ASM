using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly AuthSession _authSession;

    public LogoutModel(AuthSession authSession)
    {
        _authSession = authSession;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Clear JWT token from AuthSession
        _authSession.ClearToken();

        // Clear session
        HttpContext.Session.Clear();

        // Sign out from cookie authentication
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["Success"] = "You have been logged out successfully.";

        return RedirectToPage("/Account/Login");
    }
}
