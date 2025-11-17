using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        // If user is authenticated, redirect to Admin dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Admin/Index");
        }

        // Otherwise, redirect to login
        return RedirectToPage("/Account/Login");
    }
}
