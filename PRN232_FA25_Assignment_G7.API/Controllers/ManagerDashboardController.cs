using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/manager")]
[Authorize(Roles = "Manager")]
public class ManagerDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ManagerDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ManagerDashboardResponse>> GetDashboard(CancellationToken ct)
    {
        var totalExaminers = await _context.Examiners.CountAsync(ct);
        var totalExams = await _context.Exams.CountAsync(ct);
        var pendingGrading = await _context.Submissions
            .CountAsync(s => s.SubmissionStatus == SubmissionStatus.Pending, ct);
        var graded = await _context.Submissions
            .CountAsync(s => s.SubmissionStatus >= SubmissionStatus.Graded, ct);
        var totalSubmissions = pendingGrading + graded;
        var pendingViolations = await _context.Violations
            .CountAsync(v => v.ReviewStatus == ViolationReviewStatus.Pending, ct);

        // For double grade required, assume some logic, for now 0
        var doubleGradeRequired = 0;

        var exams = await _context.Exams
            .Include(e => e.Submissions)
            .Take(10)
            .ToListAsync(ct);

        var progress = exams.Select(e => new ExamProgress(
            e.Id,
            e.Name,
            e.Submissions.Count(s => s.SubmissionStatus >= SubmissionStatus.Graded),
            e.Submissions.Count(s => s.SubmissionStatus == SubmissionStatus.Pending)
        )).ToList();

        var response = new ManagerDashboardResponse(
            totalExams,
            totalExaminers,
            totalSubmissions,
            graded,
            pendingGrading,
            doubleGradeRequired,
            pendingViolations,
            progress
        );

        return Ok(response);
    }

    [HttpGet("grading-progress")]
    public async Task<ActionResult<List<ExamProgress>>> GetGradingProgress(CancellationToken ct)
    {
        var exams = await _context.Exams
            .Include(e => e.Submissions)
            .ToListAsync(ct);

        var progress = exams.Select(e => new ExamProgress(
            e.Id,
            e.Name,
            e.Submissions.Count(s => s.SubmissionStatus >= SubmissionStatus.Graded),
            e.Submissions.Count(s => s.SubmissionStatus == SubmissionStatus.Pending)
        )).ToList();

        return Ok(progress);
    }

    [HttpGet("submissions")]
    public async Task<ActionResult<IReadOnlyList<SubmissionResponse>>> GetSubmissions(CancellationToken ct)
    {
        var submissions = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var response = submissions.Select(s => new SubmissionResponse(
            s.Id,
            s.ExamId,
            s.Exam?.Name ?? string.Empty,
            s.StudentCode,
            s.OriginalFileName,
            s.ExtractedFolderPath,
            s.Score,
            s.CreatedAt,
            s.Violations.Count
        )).ToList();

        return Ok(response);
    }
}
