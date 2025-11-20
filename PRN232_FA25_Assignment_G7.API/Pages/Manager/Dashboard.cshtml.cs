using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN232_FA25_Assignment_G7.API.Pages.Manager;

[Authorize(Roles = "Manager")]
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}