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
public class RubricsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RubricsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("exam/{examId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<RubricResponse>>> GetByExam(Guid examId, CancellationToken ct)
    {
        var rubrics = await _context.Rubrics
            .Where(r => r.ExamId == examId)
            .ToListAsync(ct);

        var response = rubrics.Select(r => new RubricResponse(r.Id, r.ExamId, r.Criteria, r.MaxScore)).ToList();
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Moderator")]
    public async Task<ActionResult<RubricResponse>> Create([FromBody] AddRubricRequest request, CancellationToken ct)
    {
        var exam = await _context.Exams.FindAsync([request.ExamId], ct);
        if (exam == null) return BadRequest("Exam not found.");

        var rubric = new Rubric
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            Criteria = request.Criteria,
            MaxScore = request.MaxScore
        };

        _context.Rubrics.Add(rubric);
        await _context.SaveChangesAsync(ct);

        var response = new RubricResponse(rubric.Id, rubric.ExamId, rubric.Criteria, rubric.MaxScore);
        return CreatedAtAction(nameof(GetByExam), new { examId = rubric.ExamId }, response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var rubric = await _context.Rubrics.FindAsync([id], ct);
        if (rubric == null) return NotFound();

        _context.Rubrics.Remove(rubric);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
