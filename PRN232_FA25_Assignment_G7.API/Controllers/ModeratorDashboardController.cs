using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/moderator")]
[Authorize(Roles = "Moderator")]
public class ModeratorDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ModeratorDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ModeratorDashboardResponse>> GetDashboard(CancellationToken ct)
    {
        var pendingReviews = await _context.Violations
            .CountAsync(v => v.ReviewStatus == ViolationReviewStatus.Pending, ct);
        
        var zeroScoreViolations = await _context.Violations
            .CountAsync(v => v.IsZeroScore, ct);
        
        var resolvedViolations = await _context.Violations
            .CountAsync(v => v.ReviewStatus == ViolationReviewStatus.ModeratorApproved 
                          || v.ReviewStatus == ViolationReviewStatus.AdminApproved, ct);

        var pendingViolations = await _context.Violations
            .Include(v => v.Submission)
            .Where(v => v.ReviewStatus == ViolationReviewStatus.Pending)
            .OrderByDescending(v => v.CreatedAt)
            .Take(10)
            .Select(v => new PendingViolationSummary(
                v.Id,
                v.SubmissionId,
                v.Submission!.StudentCode,
                v.Type,
                v.Severity,
                v.IsZeroScore,
                v.CreatedAt
            ))
            .ToListAsync(ct);

        var response = new ModeratorDashboardResponse(
            pendingReviews,
            zeroScoreViolations,
            resolvedViolations,
            pendingViolations
        );

        return Ok(response);
    }

    [HttpGet("violations")]
    public async Task<ActionResult<IReadOnlyList<ViolationDetailResponse>>> GetViolations(CancellationToken ct)
    {
        var violations = await _context.Violations
            .Include(v => v.Submission)
            .ThenInclude(s => s!.Exam)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);

        var response = violations.Select(v => new ViolationDetailResponse(
            v.Id,
            v.SubmissionId,
            v.Submission?.StudentCode ?? string.Empty,
            v.Submission?.Exam?.Name ?? string.Empty,
            v.Type,
            v.Description,
            v.Severity,
            v.IsZeroScore,
            v.ReviewStatus.ToString(),
            v.ReviewedBy,
            v.ReviewedAt,
            v.ReviewComments,
            v.CreatedAt
        )).ToList();

        return Ok(response);
    }

    [HttpGet("violations/history")]
    public async Task<ActionResult<IReadOnlyList<ViolationDetailResponse>>> GetViolationHistory(CancellationToken ct)
    {
        var violations = await _context.Violations
            .Include(v => v.Submission)
            .ThenInclude(s => s!.Exam)
            .Where(v => v.ReviewStatus != ViolationReviewStatus.Pending)
            .OrderByDescending(v => v.ReviewedAt)
            .ToListAsync(ct);

        var response = violations.Select(v => new ViolationDetailResponse(
            v.Id,
            v.SubmissionId,
            v.Submission?.StudentCode ?? string.Empty,
            v.Submission?.Exam?.Name ?? string.Empty,
            v.Type,
            v.Description,
            v.Severity,
            v.IsZeroScore,
            v.ReviewStatus.ToString(),
            v.ReviewedBy,
            v.ReviewedAt,
            v.ReviewComments,
            v.CreatedAt
        )).ToList();

        return Ok(response);
    }
}
