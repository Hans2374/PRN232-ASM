using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin;

[Authorize]
public class IndexModel : PageModel
{
    public CurrentUser? CurrentUser { get; set; }

    public void OnGet()
    {
        CurrentUser = new CurrentUser
        {
            FullName = User.FindFirst("FullName")?.Value ?? "Unknown",
            Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown",
            Username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown"
        };
    }
}

public class CurrentUser
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
