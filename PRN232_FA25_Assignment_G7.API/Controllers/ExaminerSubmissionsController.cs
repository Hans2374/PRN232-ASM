using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Services.DTOs;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/examiner/submissions")]
[Authorize(Roles = "Examiner")]
public class ExaminerSubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExaminerSubmissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SubmissionListDto>>> GetSubmissions(
        [FromQuery] Guid? examId,
        [FromQuery] string status = "Pending",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _context.Users.FindAsync([Guid.Parse(userId)], ct);
        if (user == null) return Unauthorized();

        var examiner = await _context.Examiners
            .FirstOrDefaultAsync(e => e.Email == user.Email, ct);

        if (examiner == null) return Forbid();

        // Get exams assigned to this examiner
        var assignedExamIds = await _context.ExamExaminers
            .Where(ee => ee.ExaminerId == examiner.Id)
            .Select(ee => ee.ExamId)
            .ToListAsync(ct);

        if (!assignedExamIds.Any()) return Ok(new PagedResult<SubmissionListDto> { Items = new List<SubmissionListDto>(), PageNumber = 1, PageSize = pageSize, TotalCount = 0, TotalPages = 0 });

        var query = _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .Where(s => assignedExamIds.Contains(s.ExamId));

        // Apply filters
        if (examId.HasValue)
        {
            query = query.Where(s => s.ExamId == examId.Value);
        }

        switch (status.ToLower())
        {
            case "pending":
                query = query.Where(s => s.GradedBy == null);
                break;
            case "completed":
                query = query.Where(s => s.GradedBy == Guid.Parse(userId));
                break;
            case "all":
                // No additional filter
                break;
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var submissions = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = submissions.Select(s => new SubmissionListDto(
            s.Id,
            s.ExamId,
            s.Exam?.Name ?? string.Empty,
            s.StudentCode, // Note: We don't have student name in current schema
            s.StudentCode,
            s.OriginalFileName,
            s.Score,
            s.CreatedAt,
            s.Violations.Count
        )).ToList();

        var result = new PagedResult<SubmissionListDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionDetailDto>> GetSubmissionDetail(Guid id, CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _context.Users.FindAsync([Guid.Parse(userId)], ct);
        if (user == null) return Unauthorized();

        var examiner = await _context.Examiners
            .FirstOrDefaultAsync(e => e.Email == user.Email, ct);

        if (examiner == null) return Forbid();

        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission == null) return NotFound();

        // Verify examiner is assigned to this exam
        var isAssigned = await _context.ExamExaminers
            .AnyAsync(ee => ee.ExamId == submission.ExamId && ee.ExaminerId == examiner.Id, ct);

        if (!isAssigned) return Forbid();

        var rubric = submission.Exam?.Rubrics.Select(r => new RubricCriterionDto(
            r.Id,
            r.Criteria,
            r.MaxScore
        )).ToList() ?? new List<RubricCriterionDto>();

        var scores = new List<ScoreDto>(); // Simplified - no individual scores stored

        var violations = submission.Violations.Select(v => new ViolationDto(
            v.Id,
            v.Type,
            v.Description,
            v.CreatedAt
        )).ToList();

        var result = new SubmissionDetailDto(
            submission.Id,
            submission.StudentCode, // Using StudentCode as name for now
            submission.StudentCode,
            submission.ExamId,
            submission.Exam?.Name ?? string.Empty,
            $"/api/files/submissions/{submission.Id}", // Placeholder file URL
            violations,
            rubric,
            scores,
            submission.Score,
            submission.GradingComments,
            submission.CreatedAt
        );

        return Ok(result);
    }

    [HttpPost("{id:guid}/grade")]
    [Authorize(Roles = "Examiner")]
    public async Task<IActionResult> SubmitGrade(Guid id, [FromBody] SubmitGradeRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var userGuid = Guid.Parse(userId);

        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission == null) return NotFound();

        // Check if already graded
        if (submission.GradedBy.HasValue) return BadRequest("Submission already graded");

        // Calculate total score
        var totalScore = request.Scores.Sum(s => s.Score);
        var maxScore = submission.Exam?.Rubrics.Sum(r => r.MaxScore) ?? 0;

        if (totalScore > maxScore) return BadRequest("Total score exceeds maximum possible score");

        // Update submission (no individual scores stored)
        submission.Score = totalScore;
        submission.GradedBy = userGuid;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradingComments = request.FinalComment;
        submission.SubmissionStatus = SubmissionStatus.Graded;

        await _context.SaveChangesAsync(ct);

        return Ok(new { message = "Grade submitted successfully", totalScore });
    }
}