using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.API.SignalR;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ViolationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<SubmissionHub> _hubContext;

    public ViolationsController(ApplicationDbContext context, IHubContext<SubmissionHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet("submission/{submissionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ViolationResponse>>> GetBySubmission(Guid submissionId, CancellationToken ct)
    {
        var violations = await _context.Violations
            .Include(v => v.Submission)
            .Where(v => v.SubmissionId == submissionId)
            .ToListAsync(ct);

        var response = violations.Select(v => new ViolationResponse(
            v.Id,
            v.SubmissionId,
            v.Submission?.StudentCode ?? string.Empty,
            v.Type,
            v.Description,
            v.Severity,
            v.IsZeroScore
        )).ToList();

        return Ok(response);
    }

    [HttpPost("submission/{submissionId:guid}/detect")]
    [Authorize(Roles = "Admin,Manager,Moderator")]
    public async Task<ActionResult<IReadOnlyList<ViolationResponse>>> DetectViolations(Guid submissionId, CancellationToken ct)
    {
        var submission = await _context.Submissions
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct);

        if (submission == null) return NotFound();

        // TODO: Implement violation detection logic
        // Placeholder: detect if student code contains certain patterns
        var detectedViolations = new List<Violation>();

        // Example violation
        var violation = new Violation
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Type = "Plagiarism",
            Description = "Detected potential plagiarism.",
            Severity = 5,
            IsZeroScore = true
        };

        _context.Violations.Add(violation);
        await _context.SaveChangesAsync(ct);

        // Broadcast violation flagged
        await _hubContext.Clients.All.SendAsync("ViolationFlagged", new
        {
            SubmissionId = submissionId,
            StudentCode = submission.StudentCode,
            ViolationType = violation.Type,
            IsZeroScore = violation.IsZeroScore,
            Timestamp = DateTime.UtcNow
        }, ct);

        var response = new List<ViolationResponse>
        {
            new(violation.Id, violation.SubmissionId, submission.StudentCode, violation.Type, violation.Description, violation.Severity, violation.IsZeroScore)
        };

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var violation = await _context.Violations.FindAsync([id], ct);
        if (violation == null) return NotFound();

        _context.Violations.Remove(violation);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
