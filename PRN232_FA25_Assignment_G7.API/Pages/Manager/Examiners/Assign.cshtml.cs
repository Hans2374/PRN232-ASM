using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN232_FA25_Assignment_G7.API.Pages.Manager.Examiners;

[Authorize(Roles = "Manager")]
public class AssignModel : PageModel
{
    public void OnGet()
    {
    }
}