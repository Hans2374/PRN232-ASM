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

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ExamResponse>> Update(Guid id, [FromBody] UpdateExamRequest request, CancellationToken ct)
    {
        var exam = await _context.Exams.FindAsync([id], ct);
        if (exam == null) return NotFound();

        exam.SubjectId = request.SubjectId;
        exam.SemesterId = request.SemesterId;
        exam.Name = request.Name;
        exam.Description = request.Description;
        exam.ExamDate = request.ExamDate;

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

        return Ok(response);
    }

    [HttpGet("{id:guid}/examiners")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyList<ExamExaminerResponse>>> GetExaminers(Guid id, CancellationToken ct)
    {
        var examExists = await _context.Exams.AnyAsync(e => e.Id == id, ct);
        if (!examExists) return NotFound();

        var assignments = await _context.ExamExaminers
            .Include(ee => ee.Examiner)
            .Where(ee => ee.ExamId == id)
            .Select(ee => new ExamExaminerResponse(
                ee.ExamId,
                ee.ExaminerId,
                ee.Examiner!.FullName,
                ee.Examiner!.Email,
                ee.IsPrimaryGrader,
                ee.AssignedAt
            ))
            .ToListAsync(ct);

        return Ok(assignments);
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ExamExaminerResponse>> AssignExaminer(Guid id, [FromBody] AssignExaminerRequest request, CancellationToken ct)
    {
        var exam = await _context.Exams.FindAsync([id], ct);
        if (exam == null) return NotFound("Exam not found");

        var examiner = await _context.Examiners.FindAsync([request.ExaminerId], ct);
        if (examiner == null) return NotFound("Examiner not found");

        // Check if already assigned
        var existing = await _context.ExamExaminers
            .FirstOrDefaultAsync(ee => ee.ExamId == id && ee.ExaminerId == request.ExaminerId, ct);

        if (existing != null)
            return BadRequest("Examiner already assigned to this exam");

        var assignment = new ExamExaminer
        {
            ExamId = id,
            ExaminerId = request.ExaminerId,
            IsPrimaryGrader = request.IsPrimaryGrader,
            AssignedAt = DateTime.UtcNow
        };

        _context.ExamExaminers.Add(assignment);
        await _context.SaveChangesAsync(ct);

        var response = new ExamExaminerResponse(
            assignment.ExamId,
            assignment.ExaminerId,
            examiner.FullName,
            examiner.Email,
            assignment.IsPrimaryGrader,
            assignment.AssignedAt
        );

        return CreatedAtAction(nameof(GetExaminers), new { id }, response);
    }

    [HttpDelete("{examId:guid}/examiners/{examinerId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UnassignExaminer(Guid examId, Guid examinerId, CancellationToken ct)
    {
        var assignment = await _context.ExamExaminers
            .FirstOrDefaultAsync(ee => ee.ExamId == examId && ee.ExaminerId == examinerId, ct);

        if (assignment == null) return NotFound();

        _context.ExamExaminers.Remove(assignment);
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
            s.Violations.Any(v => v.Severity == ViolationSeverity.Critical && v.Status == ViolationStatus.Resolved),
            s.Violations.Any(v => v.Severity == ViolationSeverity.Critical && v.Status == ViolationStatus.Resolved) ? string.Join(", ", s.Violations.Where(v => v.Severity == ViolationSeverity.Critical && v.Status == ViolationStatus.Resolved).Select(v => v.ViolationType.ToString())) : string.Empty,
            s.Violations.Select(v => v.ViolationType.ToString()).ToList()
        )).ToList();

        var exportDto = new ExportResultDto(exam.Name, DateTime.UtcNow, results);

        // TODO: Implement Excel generation using ClosedXML
        return Ok(exportDto);
    }
}
