using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(ApplicationDbContext context, ILogger<AdminDashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponse>> GetDashboard(CancellationToken ct)
    {
        var totalUsers = await _context.Users.CountAsync(ct);
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive, ct);
        var totalSubjects = await _context.Subjects.CountAsync(ct);
        var totalSemesters = await _context.Semesters.CountAsync(ct);
        var totalExams = await _context.Exams.CountAsync(ct);
        var totalSubmissions = await _context.Submissions.CountAsync(ct);
        var pendingViolations = await _context.Violations
            .CountAsync(v => v.Status == ViolationStatus.New, ct);
        var pendingReviews = await _context.Submissions
            .CountAsync(s => s.ReviewStatus == ReviewStatus.AdminPending, ct);

        var recentSubmissions = await _context.Submissions
            .Include(s => s.Exam)
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .Select(s => new RecentSubmissionSummary(
                s.Id,
                s.StudentCode,
                s.Exam!.Name,
                s.CreatedAt,
                s.SubmissionStatus.ToString()
            ))
            .ToListAsync(ct);

        var response = new AdminDashboardResponse(
            totalUsers,
            activeUsers,
            totalSubjects,
            totalSemesters,
            totalExams,
            totalSubmissions,
            pendingViolations,
            pendingReviews,
            recentSubmissions
        );

        return Ok(response);
    }
}
