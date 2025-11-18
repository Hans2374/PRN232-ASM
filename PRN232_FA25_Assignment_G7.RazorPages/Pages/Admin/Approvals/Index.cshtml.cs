using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Approvals;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
