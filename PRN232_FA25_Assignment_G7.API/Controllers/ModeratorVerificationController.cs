using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FA25_Assignment_G7.Services.DTOs.Moderator;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using PRN232_FA25_Assignment_G7.Services.DTOs;
using System.Security.Claims;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/moderator/zero-scores")]
[Authorize(Roles = "Moderator")]
public class ModeratorVerificationController : ControllerBase
{
    private readonly IModeratorService _moderatorService;

    public ModeratorVerificationController(IModeratorService moderatorService)
    {
        _moderatorService = moderatorService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ZeroScoreSubmissionDto>>> GetZeroScoreSubmissions(
        [FromQuery] Guid? examId = null,
        [FromQuery] string? status = "Pending",
        [FromQuery] string? studentCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filter = new ModeratorQuery
        {
            ExamId = examId,
            Status = status,
            StudentCode = studentCode,
            Page = page,
            PageSize = pageSize
        };

        var result = await _moderatorService.GetZeroScoreSubmissionsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ZeroScoreDetailDto>> GetZeroScoreDetail(Guid id)
    {
        var result = await _moderatorService.GetZeroScoreDetailAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/verify")]
    public async Task<IActionResult> VerifyZeroScore(Guid id, [FromBody] VerifyZeroScoreRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        await _moderatorService.VerifyZeroScoreAsync(id, request, userId);
        return Ok();
    }
}