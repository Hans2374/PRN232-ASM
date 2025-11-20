using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/manager/violations")]
[Authorize(Roles = "Manager")]
public class ManagerViolationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ManagerViolationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ViolationSummary>>> GetViolations()
    {
        var violations = await _context.Violations
            .Include(v => v.Submission)
            .Where(v => v.Status == ViolationStatus.New)
            .Join(_context.Examiners,
                v => v.Submission.GradedBy,
                e => e.Id,
                (v, e) => new ViolationSummary(
                    v.Id,
                    v.SubmissionId,
                    e.FullName,
                    v.Description,
                    v.Status.ToString()
                )).ToListAsync();
        return Ok(violations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ViolationDetail>> GetViolation(Guid id)
    {
        var violation = await _context.Violations
            .Include(v => v.Submission)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (violation == null) return NotFound();

        var examiner = await _context.Examiners.FindAsync(violation.Submission.GradedBy);

        var detail = new ViolationDetail(
            violation.Id,
            violation.SubmissionId,
            examiner?.FullName ?? "Unknown",
            violation.Description,
            violation.ViolationType.ToString(),
            new string[0], // no evidence urls
            violation.Status.ToString()
        );
        return Ok(detail);
    }

    [HttpPost("{id:guid}/decision")]
    public async Task<IActionResult> SubmitDecision(Guid id, [FromBody] ViolationDecisionRequest request)
    {
        var violation = await _context.Violations.FindAsync(id);
        if (violation == null) return NotFound();

        violation.Status = request.Decision switch
        {
            "approve" => ViolationStatus.Resolved,
            "reject" => ViolationStatus.Escalated,
            "escalate" => ViolationStatus.Escalated,
            _ => violation.Status
        };

        await _context.SaveChangesAsync();
        return Ok();
    }

    public record ViolationSummary(Guid ViolationId, Guid? SubmissionId, string ExaminerName, string Reason, string Status);
    public record ViolationDetail(Guid ViolationId, Guid? SubmissionId, string ExaminerName, string Reason, string Details, string[] EvidenceUrls, string Status);
    public record ViolationDecisionRequest(string Decision, string Comment);
}