using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using PRN232_FA25_Assignment_G7.Repositories;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Manager,Moderator")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("exams/{examId:guid}/results/export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportExamResults(Guid examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null) return NotFound();

        var submissions = await _context.Submissions
            .Include(s => s.Violations)
            .Where(s => s.ExamId == examId)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Exam Results");

        // Headers
        worksheet.Cell(1, 1).Value = "Student Code";
        worksheet.Cell(1, 2).Value = "Score";
        worksheet.Cell(1, 3).Value = "Second Score";
        worksheet.Cell(1, 4).Value = "Final Score";
        worksheet.Cell(1, 5).Value = "Status";
        worksheet.Cell(1, 6).Value = "Violations Count";

        for (int i = 0; i < submissions.Count; i++)
        {
            var sub = submissions[i];
            worksheet.Cell(i + 2, 1).Value = sub.StudentCode;
            worksheet.Cell(i + 2, 2).Value = sub.Score;
            worksheet.Cell(i + 2, 3).Value = sub.SecondScore;
            worksheet.Cell(i + 2, 4).Value = sub.Score ?? 0; // Simplified
            worksheet.Cell(i + 2, 5).Value = sub.SubmissionStatus.ToString();
            worksheet.Cell(i + 2, 6).Value = sub.Violations.Count;
        }

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        var bytes = ms.ToArray();
        ms.Dispose();

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Exam_{exam.Name}_Results.xlsx");
    }

    [HttpGet("violations/export")]
    [Authorize(Roles = "Admin,Manager,Moderator")]
    public async Task<IActionResult> ExportViolations()
    {
        var violations = await _context.Violations
            .Include(v => v.Submission)
            .ThenInclude(s => s.Exam)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Violations");

        worksheet.Cell(1, 1).Value = "Exam";
        worksheet.Cell(1, 2).Value = "Student Code";
        worksheet.Cell(1, 3).Value = "Type";
        worksheet.Cell(1, 4).Value = "Description";
        worksheet.Cell(1, 5).Value = "Severity";
        worksheet.Cell(1, 6).Value = "Is Zero Score";

        for (int i = 0; i < violations.Count; i++)
        {
            var v = violations[i];
            worksheet.Cell(i + 2, 1).Value = v.Submission?.Exam?.Name ?? "";
            worksheet.Cell(i + 2, 2).Value = v.Submission?.StudentCode ?? "";
            worksheet.Cell(i + 2, 3).Value = v.ViolationType.ToString();
            worksheet.Cell(i + 2, 4).Value = v.Description;
            worksheet.Cell(i + 2, 5).Value = v.Severity.ToString();
            worksheet.Cell(i + 2, 6).Value = v.Status.ToString();
        }

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        var bytes = ms.ToArray();
        ms.Dispose();

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Violations_Report.xlsx");
    }
}