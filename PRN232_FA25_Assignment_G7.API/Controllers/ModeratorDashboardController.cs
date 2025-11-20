using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using System.Security.Claims;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/moderator")]
[Authorize(Roles = "Moderator")]
public class ModeratorDashboardController : ControllerBase
{
    private readonly IModeratorService _moderatorService;

    public ModeratorDashboardController(IModeratorService moderatorService)
    {
        _moderatorService = moderatorService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<Services.DTOs.Moderator.ModeratorDashboardResponse>> GetDashboard()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        var result = await _moderatorService.GetDashboardAsync(userId);
        return Ok(result);
    }
}
