using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExaminersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExaminersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyList<ExaminerResponse>>> GetAll(CancellationToken ct)
    {
        var examiners = await _context.Examiners
            .Include(e => e.ExaminerSubjects)
            .ThenInclude(es => es.Subject)
            .ToListAsync(ct);

        var response = examiners.Select(e => new ExaminerResponse(
            e.Id,
            e.FullName,
            e.Email,
            e.ExaminerSubjects.Select(es => es.Subject?.Name ?? string.Empty).ToList()
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Moderator")]
    public async Task<ActionResult<ExaminerResponse>> Get(Guid id, CancellationToken ct)
    {
        var examiner = await _context.Examiners
            .Include(e => e.ExaminerSubjects)
            .ThenInclude(es => es.Subject)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (examiner == null) return NotFound();

        var response = new ExaminerResponse(
            examiner.Id,
            examiner.FullName,
            examiner.Email,
            examiner.ExaminerSubjects.Select(es => es.Subject?.Name ?? string.Empty).ToList()
        );

        return Ok(response);
    }
}
