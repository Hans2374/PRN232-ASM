using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Services.DTOs;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/examiner/double-grading")]
[Authorize(Roles = "Examiner")]
public class ExaminerDoubleGradingController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExaminerDoubleGradingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<DoubleGradingTaskDto>>> GetDoubleGradingTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var userGuid = Guid.Parse(userId);

        var query = _context.Submissions
            .Include(s => s.Exam)
            .Where(s => s.SecondGradedBy == userGuid);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var submissions = await query
            .OrderByDescending(s => s.SecondGradedAt ?? s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = submissions.Select(s => new DoubleGradingTaskDto(
            s.Id,
            s.StudentCode, // Using StudentCode as name
            s.StudentCode,
            s.Exam?.Name ?? string.Empty,
            s.Score, // Primary score
            s.SecondScore, // Secondary score
            s.SecondGradedAt ?? s.CreatedAt
        )).ToList();

        var result = new PagedResult<DoubleGradingTaskDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DoubleGradingDetailDto>> GetDoubleGradingDetail(Guid id, CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var userGuid = Guid.Parse(userId);

        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == id && s.SecondGradedBy == userGuid, ct);

        if (submission == null) return NotFound();

        var rubric = submission.Exam?.Rubrics.Select(r => new RubricCriterionDto(
            r.Id,
            r.Criteria,
            r.MaxScore
        )).ToList() ?? new List<RubricCriterionDto>();

        var scores = new List<ScoreDto>(); // Simplified - no individual scores stored
        var primaryScores = new List<ScoreDto>(); // Simplified - no individual scores stored

        var violations = submission.Violations.Select(v => new PRN232_FA25_Assignment_G7.API.DTOs.ViolationDto(
            v.Id,
            v.ViolationType.ToString(),
            v.Description,
            v.CreatedAt
        )).ToList();

        var result = new DoubleGradingDetailDto(
            submission.Id,
            submission.StudentCode,
            submission.StudentCode,
            submission.ExamId,
            submission.Exam?.Name ?? string.Empty,
            $"/api/files/submissions/{submission.Id}",
            violations,
            rubric,
            scores,
            primaryScores,
            submission.Score,
            submission.SecondScore,
            submission.SecondGradingComments,
            submission.CreatedAt,
            submission.SecondGradedAt ?? submission.CreatedAt
        );

        return Ok(result);
    }

    [HttpPost("{id:guid}/grade")]
    public async Task<IActionResult> SubmitDoubleGrade(Guid id, [FromBody] SubmitDoubleGradingRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var userGuid = Guid.Parse(userId);

        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .FirstOrDefaultAsync(s => s.Id == id && s.SecondGradedBy == userGuid, ct);

        if (submission == null) return NotFound();

        // Check if already double graded
        if (submission.SecondGradedAt.HasValue) return BadRequest("Submission already double graded");

        // Calculate total score
        var totalScore = request.Scores.Sum(s => s.Score);
        var maxScore = submission.Exam?.Rubrics.Sum(r => r.MaxScore) ?? 0;

        if (totalScore > maxScore) return BadRequest("Total score exceeds maximum possible score");

        // Update submission
        submission.SecondScore = totalScore;
        submission.SecondGradedAt = DateTime.UtcNow;
        submission.SecondGradingComments = request.FinalComment;

        // Check for score discrepancy and create complaint if needed
        var primaryScore = submission.Score ?? 0;
        var difference = Math.Abs(primaryScore - totalScore);

        if (difference > 10) // Threshold for complaint
        {
            var complaint = new Complaint
            {
                SubmissionId = id,
                StudentCode = submission.StudentCode,
                Title = "Score Discrepancy",
                Description = $"Score discrepancy detected. Primary: {primaryScore}, Secondary: {totalScore}, Difference: {difference}",
                Status = ComplaintStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Complaints.Add(complaint);
        }

        await _context.SaveChangesAsync(ct);

        return Ok();
    }
}