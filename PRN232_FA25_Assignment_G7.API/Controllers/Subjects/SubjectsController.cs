using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Repositories.Repositories.Interfaces;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectRepository _repository;

    public SubjectsController(ISubjectRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SubjectResponse>>> GetAll(CancellationToken ct)
    {
        var subjects = await _repository.ListAsync(ct: ct);
        var response = subjects.Select(s => new SubjectResponse(s.Id, s.Code, s.Name)).ToList();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<SubjectResponse>> Get(Guid id, CancellationToken ct)
    {
        var subject = await _repository.GetByIdAsync(id, ct);
        if (subject == null) return NotFound();
        return Ok(new SubjectResponse(subject.Id, subject.Code, subject.Name));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SubjectResponse>> Create([FromBody] CreateSubjectRequest request, CancellationToken ct)
    {
        var existing = await _repository.GetByCodeAsync(request.Code, ct);
        if (existing != null) return BadRequest("Subject code already exists.");

        var subject = new Subject { Id = Guid.NewGuid(), Code = request.Code, Name = request.Name };
        await _repository.AddAsync(subject, ct);
        await _repository.SaveChangesAsync(ct);

        var response = new SubjectResponse(subject.Id, subject.Code, subject.Name);
        return CreatedAtAction(nameof(Get), new { id = subject.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SubjectResponse>> Update(Guid id, [FromBody] UpdateSubjectRequest request, CancellationToken ct)
    {
        var subject = await _repository.GetByIdAsync(id, ct);
        if (subject == null) return NotFound();

        subject.Code = request.Code;
        subject.Name = request.Name;
        _repository.Update(subject);
        await _repository.SaveChangesAsync(ct);

        return Ok(new SubjectResponse(subject.Id, subject.Code, subject.Name));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var subject = await _repository.GetByIdAsync(id, ct);
        if (subject == null) return NotFound();

        _repository.Remove(subject);
        await _repository.SaveChangesAsync(ct);
        return NoContent();
    }
}
