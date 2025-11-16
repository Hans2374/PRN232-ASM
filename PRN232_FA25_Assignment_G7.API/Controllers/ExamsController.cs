using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ExamResponse>>> GetAll(CancellationToken ct)
    {
        var exams = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.Semester)
            .ToListAsync(ct);

        var response = exams.Select(e => new ExamResponse(
            e.Id,
            e.SubjectId,
            e.Subject?.Name ?? string.Empty,
            e.SemesterId,
            e.Semester?.Name ?? string.Empty,
            e.Name,
            e.Description,
            e.ExamDate
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExamDetailResponse>> Get(Guid id, CancellationToken ct)
    {
        var exam = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.Semester)
            .Include(e => e.Rubrics)
            .Include(e => e.Submissions)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (exam == null) return NotFound();

        var response = new ExamDetailResponse(
            exam.Id,
            exam.Name,
            exam.Description,
            exam.ExamDate,
            new SubjectResponse(exam.Subject!.Id, exam.Subject.Code, exam.Subject.Name),
            new SemesterResponse(exam.Semester!.Id, exam.Semester.Name, exam.Semester.StartDate, exam.Semester.EndDate),
            exam.Rubrics.Select(r => new RubricResponse(r.Id, r.ExamId, r.Criteria, r.MaxScore)).ToList(),
            exam.Submissions.Count
        );

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ExamResponse>> Create([FromBody] CreateExamRequest request, CancellationToken ct)
    {
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            SubjectId = request.SubjectId,
            SemesterId = request.SemesterId,
            Name = request.Name,
            Description = request.Description,
            ExamDate = request.ExamDate
        };

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync(ct);

        var subject = await _context.Subjects.FindAsync([request.SubjectId], ct);
        var semester = await _context.Semesters.FindAsync([request.SemesterId], ct);

        var response = new ExamResponse(
            exam.Id,
            exam.SubjectId,
            subject?.Name ?? string.Empty,
            exam.SemesterId,
            semester?.Name ?? string.Empty,
            exam.Name,
            exam.Description,
            exam.ExamDate
        );

        return CreatedAtAction(nameof(Get), new { id = exam.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var exam = await _context.Exams.FindAsync([id], ct);
        if (exam == null) return NotFound();

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/export-results")]
    [Authorize(Roles = "Admin,Manager,Moderator")]
    public async Task<IActionResult> ExportResults(Guid id, CancellationToken ct)
    {
        var exam = await _context.Exams
            .Include(e => e.Submissions)
            .ThenInclude(s => s.Violations)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (exam == null) return NotFound();

        var results = exam.Submissions.Select(s => new StudentResult(
            s.StudentCode,
            s.Score,
            s.Violations.Any(v => v.IsZeroScore),
            s.Violations.Any(v => v.IsZeroScore) ? string.Join(", ", s.Violations.Where(v => v.IsZeroScore).Select(v => v.Type)) : string.Empty,
            s.Violations.Select(v => v.Type).ToList()
        )).ToList();

        var exportDto = new ExportResultDto(exam.Name, DateTime.UtcNow, results);

        // TODO: Implement Excel generation using ClosedXML
        return Ok(exportDto);
    }
}
