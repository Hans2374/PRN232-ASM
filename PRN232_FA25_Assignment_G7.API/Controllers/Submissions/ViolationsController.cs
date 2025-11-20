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

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Moderator")]
    public async Task<ActionResult<IReadOnlyList<ViolationResponse>>> GetAll(CancellationToken ct)
    {
        var violations = await _context.Violations
            .Include(v => v.Submission)
            .OrderByDescending(v => v.CreatedAt)
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

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Moderator")]
    public async Task<ActionResult<ViolationDetailResponse>> Get(Guid id, CancellationToken ct)
    {
        var violation = await _context.Violations
            .Include(v => v.Submission)
            .ThenInclude(s => s!.Exam)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (violation == null) return NotFound();

        var response = new ViolationDetailResponse(
            violation.Id,
            violation.SubmissionId,
            violation.Submission?.StudentCode ?? string.Empty,
            violation.Submission?.Exam?.Name ?? string.Empty,
            violation.Type,
            violation.Description,
            violation.Severity,
            violation.IsZeroScore,
            violation.ReviewStatus.ToString(),
            violation.ReviewedBy,
            violation.ReviewedAt,
            violation.ReviewComments,
            violation.CreatedAt
        );

        return Ok(response);
    }

    [HttpPost("{id:guid}/approve-moderator")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> ApproveAsModerator(Guid id, [FromBody] ReviewViolationRequest request, CancellationToken ct)
    {
        var violation = await _context.Violations.FindAsync([id], ct);
        if (violation == null) return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        violation.ReviewStatus = request.Approve 
            ? ViolationReviewStatus.ModeratorApproved 
            : ViolationReviewStatus.ModeratorRejected;
        violation.ReviewedBy = Guid.Parse(userId);
        violation.ReviewedAt = DateTime.UtcNow;
        violation.ReviewComments = request.Comments;

        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.All.SendAsync("ViolationReviewed", new
        {
            ViolationId = violation.Id,
            Status = violation.ReviewStatus.ToString(),
            Timestamp = DateTime.UtcNow
        }, ct);

        return Ok();
    }

    [HttpPost("{id:guid}/approve-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveAsAdmin(Guid id, [FromBody] ReviewViolationRequest request, CancellationToken ct)
    {
        var violation = await _context.Violations.FindAsync([id], ct);
        if (violation == null) return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        violation.ReviewStatus = request.Approve 
            ? ViolationReviewStatus.AdminApproved 
            : ViolationReviewStatus.AdminRejected;
        violation.ReviewedBy = Guid.Parse(userId);
        violation.ReviewedAt = DateTime.UtcNow;
        violation.ReviewComments = request.Comments;

        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.All.SendAsync("ViolationReviewed", new
        {
            ViolationId = violation.Id,
            Status = violation.ReviewStatus.ToString(),
            Timestamp = DateTime.UtcNow
        }, ct);

        return Ok();
    }

    [HttpPost("{id:guid}/confirm-zero")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> ConfirmZeroScore(Guid id, [FromBody] ConfirmZeroScoreRequest request, CancellationToken ct)
    {
        var violation = await _context.Violations
            .Include(v => v.Submission)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (violation == null) return NotFound();

        violation.IsZeroScore = request.Confirm;
        violation.ReviewComments = request.Comments;

        if (request.Confirm && violation.Submission != null)
        {
            violation.Submission.Score = 0;
            violation.Submission.ReviewStatus = ReviewStatus.Completed;
        }

        await _context.SaveChangesAsync(ct);

        return Ok();
    }

    [HttpPost("report")]
    [Authorize(Roles = "Examiner")]
    public async Task<ActionResult<ViolationResponse>> Report([FromBody] ReportViolationRequest request, CancellationToken ct)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, ct);

        if (submission == null) return NotFound("Submission not found");

        var violation = new Violation
        {
            Id = Guid.NewGuid(),
            SubmissionId = request.SubmissionId,
            Type = request.Type,
            Description = request.Description,
            Severity = request.Severity,
            IsZeroScore = request.IsZeroScore,
            ReviewStatus = ViolationReviewStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Violations.Add(violation);
        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.All.SendAsync("ViolationFlagged", new
        {
            SubmissionId = request.SubmissionId,
            StudentCode = submission.StudentCode,
            ViolationType = request.Type,
            IsZeroScore = request.IsZeroScore,
            Timestamp = DateTime.UtcNow
        }, ct);

        var response = new ViolationResponse(
            violation.Id,
            violation.SubmissionId,
            submission.StudentCode,
            violation.Type,
            violation.Description,
            violation.Severity,
            violation.IsZeroScore
        );

        return CreatedAtAction(nameof(Get), new { id = violation.Id }, response);
    }
}
