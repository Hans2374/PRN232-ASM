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

        // Return examiners that are assigned to this exam via the join table
        var assigned = await _context.ExamExaminers
            .Include(ee => ee.Examiner)
            .Where(ee => ee.ExamId == id)
            .Select(ee => ee.Examiner!)
            .ToListAsync();

        return Ok(assigned);
    }

    [HttpGet("{id:guid}/unassigned-examiners")]
    public async Task<ActionResult<IReadOnlyList<Examiner>>> GetUnassignedExaminers(Guid id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        var assignedIds = await _context.ExamExaminers
            .Where(ee => ee.ExamId == id)
            .Select(ee => ee.ExaminerId)
            .ToListAsync();

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

        // Validate examiners exist
        var examiners = await _context.Examiners
            .Where(e => request.ExaminerIds.Contains(e.Id))
            .ToListAsync();

        if (examiners.Count != request.ExaminerIds.Count)
            return BadRequest("Some examiners not found");

        var added = 0;
        foreach (var examinerId in request.ExaminerIds)
        {
            var exists = await _context.ExamExaminers.AnyAsync(ee => ee.ExamId == id && ee.ExaminerId == examinerId);
            if (exists) continue;

            var assignment = new ExamExaminer
            {
                ExamId = id,
                ExaminerId = examinerId,
                AssignedAt = DateTime.UtcNow,
                IsPrimaryGrader = true
            };

            _context.ExamExaminers.Add(assignment);
            added++;
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, assigned = added });
    }

    public record AssignExaminersRequest(List<Guid> ExaminerIds);
}