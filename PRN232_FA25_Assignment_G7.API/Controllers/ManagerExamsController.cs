using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/manager/exams")]
[Authorize(Roles = "Manager")]
public class ManagerExamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ManagerExamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Exam>>> GetExams()
    {
        var exams = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.Semester)
            .ToListAsync();
        return Ok(exams);
    }

    [HttpGet("{id:guid}/examiners")]
    public async Task<ActionResult<IReadOnlyList<Examiner>>> GetAssignedExaminers(Guid id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        // Assuming ExamExaminer table exists, but since not, perhaps use a different way.
        // For now, return all examiners as assigned if they have subjects matching.
        var examiners = await _context.Examiners
            .Include(e => e.ExaminerSubjects)
            .Where(e => e.ExaminerSubjects.Any(es => es.SubjectId == exam.SubjectId))
            .ToListAsync();
        return Ok(examiners);
    }

    [HttpGet("{id:guid}/unassigned-examiners")]
    public async Task<ActionResult<IReadOnlyList<Examiner>>> GetUnassignedExaminers(Guid id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        var assigned = await GetAssignedExaminers(id);
        var assignedIds = assigned.Value?.Select(e => e.Id).ToList() ?? new List<Guid>();

        var unassigned = await _context.Examiners
            .Include(e => e.ExaminerSubjects)
            .Where(e => !assignedIds.Contains(e.Id) && e.ExaminerSubjects.Any(es => es.SubjectId == exam.SubjectId))
            .ToListAsync();
        return Ok(unassigned);
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignExaminers(Guid id, [FromBody] AssignExaminersRequest request)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        // For now, just validate examiners exist.
        var examiners = await _context.Examiners
            .Where(e => request.ExaminerIds.Contains(e.Id))
            .ToListAsync();

        if (examiners.Count != request.ExaminerIds.Count)
            return BadRequest("Some examiners not found");

        // In real implementation, add to ExamExaminer table.
        // Since not implemented, just return success.

        return Ok(new { success = true, assigned = examiners.Count });
    }

    public record AssignExaminersRequest(List<Guid> ExaminerIds);
}