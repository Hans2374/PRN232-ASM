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
public class SubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<SubmissionHub> _hubContext;
    private readonly ILogger<SubmissionsController> _logger;

    public SubmissionsController(
        ApplicationDbContext context,
        IHubContext<SubmissionHub> hubContext,
        ILogger<SubmissionsController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionResponse>> Get(Guid id, CancellationToken ct)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission == null) return NotFound();

        var response = new SubmissionResponse(
            submission.Id,
            submission.ExamId,
            submission.Exam?.Name ?? string.Empty,
            submission.StudentCode,
            submission.OriginalFileName,
            submission.ExtractedFolderPath,
            submission.Score,
            submission.CreatedAt,
            submission.Violations.Count
        );

        return Ok(response);
    }

    [HttpGet("exam/{examId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SubmissionResponse>>> GetByExam(Guid examId, CancellationToken ct)
    {
        var submissions = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .Where(s => s.ExamId == examId)
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

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Manager,Moderator,Examiner")]
    [RequestSizeLimit(100_000_000)] // 100MB
    public async Task<ActionResult<SubmissionResponse>> Upload([FromForm] ProcessSubmissionRequest request, CancellationToken ct)
    {
        var exam = await _context.Exams.FindAsync([request.ExamId], ct);
        if (exam == null) return BadRequest("Exam not found.");

        // TODO: Implement file processing (extract, scan for violations)
        var extractedPath = $"/uploads/{request.ExamId}/{request.StudentCode}/{DateTime.UtcNow:yyyyMMddHHmmss}";

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            StudentCode = request.StudentCode,
            OriginalFileName = request.File.FileName,
            ExtractedFolderPath = extractedPath,
            CreatedAt = DateTime.UtcNow
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync(ct);

        // Broadcast via SignalR
        await _hubContext.Clients.All.SendAsync("SubmissionUploaded", new
        {
            SubmissionId = submission.Id,
            StudentCode = submission.StudentCode,
            ExamName = exam.Name,
            Timestamp = DateTime.UtcNow
        }, ct);

        var response = new SubmissionResponse(
            submission.Id,
            submission.ExamId,
            exam.Name,
            submission.StudentCode,
            submission.OriginalFileName,
            submission.ExtractedFolderPath,
            submission.Score,
            submission.CreatedAt,
            0
        );

        return CreatedAtAction(nameof(Get), new { id = submission.Id }, response);
    }

    [HttpPost("{id:guid}/score")]
    [Authorize(Roles = "Admin,Manager,Moderator,Examiner")]
    public async Task<ActionResult<SubmissionScoringResult>> Score(Guid id, [FromBody] decimal score, CancellationToken ct)
    {
        var submission = await _context.Submissions
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission == null) return NotFound();

        var hasZeroScoreViolation = submission.Violations.Any(v => v.IsZeroScore);
        if (hasZeroScoreViolation)
        {
            submission.Score = 0;
            await _context.SaveChangesAsync(ct);

            await _hubContext.Clients.All.SendAsync("SubmissionGraded", new
            {
                SubmissionId = submission.Id,
                StudentCode = submission.StudentCode,
                Score = 0m,
                Timestamp = DateTime.UtcNow
            }, ct);

            return Ok(new SubmissionScoringResult(submission.Id, 0, true, "Zero score due to violation."));
        }

        submission.Score = score;
        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.All.SendAsync("SubmissionGraded", new
        {
            SubmissionId = submission.Id,
            StudentCode = submission.StudentCode,
            Score = score,
            Timestamp = DateTime.UtcNow
        }, ct);

        return Ok(new SubmissionScoringResult(submission.Id, score, false, string.Empty));
    }
}
