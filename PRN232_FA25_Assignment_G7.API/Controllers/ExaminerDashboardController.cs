using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/examiner")]
[Authorize(Roles = "Examiner")]
public class ExaminerDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExaminerDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ExaminerDashboardResponse>> GetDashboard(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // Note: We need to link User to Examiner - for now using email matching
        var user = await _context.Users.FindAsync([Guid.Parse(userId)], ct);
        if (user == null) return Unauthorized();

        var examiner = await _context.Examiners
            .FirstOrDefaultAsync(e => e.Email == user.Email, ct);

        if (examiner == null) 
            return Ok(new ExaminerDashboardResponse(0, 0, 0, new List<AssignedExamSummary>()));

        var assignedExams = await _context.ExamExaminers
            .Include(ee => ee.Exam)
            .ThenInclude(e => e!.Subject)
            .Include(ee => ee.Exam)
            .ThenInclude(e => e!.Submissions)
            .Where(ee => ee.ExaminerId == examiner.Id)
            .ToListAsync(ct);

        var examSummaries = assignedExams.Select(ee => new AssignedExamSummary(
            ee.ExamId,
            ee.Exam!.Name,
            ee.Exam.Subject!.Name,
            ee.Exam.ExamDate,
            ee.Exam.Submissions.Count,
            ee.Exam.Submissions.Count(s => s.GradedBy == Guid.Parse(userId) || s.SecondGradedBy == Guid.Parse(userId)),
            ee.IsPrimaryGrader
        )).ToList();

        var totalPending = assignedExams
            .Sum(ee => ee.Exam!.Submissions.Count(s => s.GradedBy == null));

        var totalGraded = assignedExams
            .Sum(ee => ee.Exam!.Submissions.Count(s => s.GradedBy == Guid.Parse(userId) || s.SecondGradedBy == Guid.Parse(userId)));

        var response = new ExaminerDashboardResponse(
            assignedExams.Count,
            totalPending,
            totalGraded,
            examSummaries
        );

        return Ok(response);
    }

    [HttpGet("exams")]
    public async Task<ActionResult<IReadOnlyList<AssignedExamSummary>>> GetAssignedExams(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _context.Users.FindAsync([Guid.Parse(userId)], ct);
        if (user == null) return Unauthorized();

        var examiner = await _context.Examiners
            .FirstOrDefaultAsync(e => e.Email == user.Email, ct);

        if (examiner == null) return Ok(new List<AssignedExamSummary>());

        var assignedExams = await _context.ExamExaminers
            .Include(ee => ee.Exam)
            .ThenInclude(e => e!.Subject)
            .Include(ee => ee.Exam)
            .ThenInclude(e => e!.Submissions)
            .Where(ee => ee.ExaminerId == examiner.Id)
            .ToListAsync(ct);

        var response = assignedExams.Select(ee => new AssignedExamSummary(
            ee.ExamId,
            ee.Exam!.Name,
            ee.Exam.Subject!.Name,
            ee.Exam.ExamDate,
            ee.Exam.Submissions.Count,
            ee.Exam.Submissions.Count(s => s.GradedBy == Guid.Parse(userId) || s.SecondGradedBy == Guid.Parse(userId)),
            ee.IsPrimaryGrader
        )).ToList();

        return Ok(response);
    }

    [HttpGet("exams/{examId:guid}/submissions")]
    public async Task<ActionResult<IReadOnlyList<SubmissionResponse>>> GetExamSubmissions(Guid examId, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _context.Users.FindAsync([Guid.Parse(userId)], ct);
        if (user == null) return Unauthorized();

        var examiner = await _context.Examiners
            .FirstOrDefaultAsync(e => e.Email == user.Email, ct);

        if (examiner == null) return Forbid();

        // Verify examiner is assigned to this exam
        var isAssigned = await _context.ExamExaminers
            .AnyAsync(ee => ee.ExamId == examId && ee.ExaminerId == examiner.Id, ct);

        if (!isAssigned) return Forbid();

        var submissions = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .Where(s => s.ExamId == examId)
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

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<SubmissionResponse>>> GetGradingHistory(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var userGuid = Guid.Parse(userId);

        var submissions = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .Where(s => s.GradedBy == userGuid || s.SecondGradedBy == userGuid)
            .OrderByDescending(s => s.GradedAt ?? s.SecondGradedAt)
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
