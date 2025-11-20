using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FA25_Assignment_G7.Services.DTOs.Moderator;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using PRN232_FA25_Assignment_G7.Services.DTOs;
using System.Security.Claims;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/moderator/complaints")]
[Authorize(Roles = "Moderator")]
public class ModeratorComplaintsController : ControllerBase
{
    private readonly IModeratorService _moderatorService;

    public ModeratorComplaintsController(IModeratorService moderatorService)
    {
        _moderatorService = moderatorService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ComplaintSummaryDto>>> GetComplaints(
        [FromQuery] string? status = "Pending",
        [FromQuery] Guid? examId = null,
        [FromQuery] string? studentCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filter = new ModeratorQuery
        {
            Status = status,
            ExamId = examId,
            StudentCode = studentCode,
            Page = page,
            PageSize = pageSize
        };

        var result = await _moderatorService.GetComplaintsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ComplaintDetailDto>> GetComplaint(Guid id)
    {
        var result = await _moderatorService.GetComplaintAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/decision")]
    public async Task<IActionResult> DecideComplaint(Guid id, [FromBody] DecisionDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        await _moderatorService.DecideComplaintAsync(id, dto, userId);
        return Ok();
    }
}