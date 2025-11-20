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
public class SemestersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SemestersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SemesterResponse>>> GetAll(CancellationToken ct)
    {
        var semesters = await _context.Semesters.ToListAsync(ct);
        var response = semesters.Select(s => new SemesterResponse(s.Id, s.Name, s.StartDate, s.EndDate)).ToList();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<SemesterResponse>> Get(Guid id, CancellationToken ct)
    {
        var semester = await _context.Semesters.FindAsync([id], ct);
        if (semester == null) return NotFound();
        return Ok(new SemesterResponse(semester.Id, semester.Name, semester.StartDate, semester.EndDate));
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SemesterResponse>>> GetActive(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var semesters = await _context.Semesters
            .Where(s => s.StartDate <= now && s.EndDate >= now)
            .ToListAsync(ct);
        var response = semesters.Select(s => new SemesterResponse(s.Id, s.Name, s.StartDate, s.EndDate)).ToList();
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SemesterResponse>> Create([FromBody] CreateSemesterRequest request, CancellationToken ct)
    {
        if (request.EndDate <= request.StartDate)
            return BadRequest("End date must be after start date.");

        var semester = new Semester
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync(ct);

        var response = new SemesterResponse(semester.Id, semester.Name, semester.StartDate, semester.EndDate);
        return CreatedAtAction(nameof(Get), new { id = semester.Id }, response);
    }
}
