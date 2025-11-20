using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.Services.DTOs.Moderator;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;
using PRN232_FA25_Assignment_G7.Services.DTOs;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class ModeratorService : IModeratorService
{
    private readonly ApplicationDbContext _context;

    public ModeratorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModeratorDashboardResponse> GetDashboardAsync(Guid moderatorId)
    {
        // Total exams under review (double-graded but not finalized)
        var totalExamsUnderReview = await _context.Submissions
            .CountAsync(s => s.SubmissionStatus == SubmissionStatus.DoubleGraded &&
                           s.ReviewStatus != ReviewStatus.Completed);

        // Total pending complaints
        // var totalPendingComplaints = await _context.Complaints
        //     .CountAsync(c => c.Status == ComplaintStatus.Pending);
        var totalPendingComplaints = 0;

        // Total zero-score submissions pending verification
        var totalZeroScorePending = await _context.Violations
            .CountAsync(v => v.ViolationType == ViolationType.InvalidFormat && v.Status == ViolationStatus.New);

        // Recent discrepancies (submissions with different scores)
        var recentDiscrepancies = await _context.Submissions
            .Where(s => s.Score.HasValue && s.SecondScore.HasValue && s.Score != s.SecondScore)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new ExamDiscrepancyDto(
                s.Id,
                s.Exam!.Name,
                s.Score.Value,
                s.SecondScore.Value,
                Math.Abs(s.Score.Value - s.SecondScore.Value)
            ))
            .ToListAsync();

        // Top issues
        var topIssues = new List<TopIssueDto>
        {
            new("Double-Grading Discrepancy", await _context.Submissions
                .CountAsync(s => s.Score.HasValue && s.SecondScore.HasValue && s.Score != s.SecondScore)),
            new("Violation Flagged", await _context.Violations.CountAsync(v => v.Status == ViolationStatus.New))
        };

        return new ModeratorDashboardResponse(
            totalExamsUnderReview,
            totalPendingComplaints,
            totalZeroScorePending,
            recentDiscrepancies,
            topIssues
        );
    }

    public async Task<PagedResult<ComplaintSummaryDto>> GetComplaintsAsync(ModeratorQuery filter)
    {
        // Temporary implementation without database
        return new PagedResult<ComplaintSummaryDto>
        {
            Items = new List<ComplaintSummaryDto>(),
            PageNumber = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = 0,
            TotalPages = 0
        };
    }

    public async Task<ComplaintDetailDto> GetComplaintAsync(Guid id)
    {
        // Temporary implementation without database
        throw new KeyNotFoundException("Complaint not found");
    }

    public async Task DecideComplaintAsync(Guid id, DecisionDto dto, Guid moderatorId)
    {
        // Temporary implementation without database
        throw new KeyNotFoundException("Complaint not found");
    }

    public async Task<PagedResult<ZeroScoreSubmissionDto>> GetZeroScoreSubmissionsAsync(ModeratorQuery filter)
    {
        var query = _context.Violations
            .Include(v => v.Submission)
            .ThenInclude(s => s!.Exam)
            .Where(v => v.ViolationType == ViolationType.InvalidFormat)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All")
        {
            if (Enum.TryParse<ViolationStatus>(filter.Status, out var status))
            {
                query = query.Where(v => v.Status == status);
            }
        }

        if (filter.ExamId.HasValue)
        {
            query = query.Where(v => v.Submission!.ExamId == filter.ExamId.Value);
        }

        if (!string.IsNullOrEmpty(filter.StudentCode))
        {
            query = query.Where(v => v.Submission!.StudentCode.Contains(filter.StudentCode));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(v => new ZeroScoreSubmissionDto(
                v.Id,
                v.SubmissionId,
                v.Submission!.StudentCode,
                v.Submission!.Exam!.Name,
                v.ViolationType.ToString(),
                v.Description,
                v.CreatedAt
            ))
            .ToListAsync();

        return new PagedResult<ZeroScoreSubmissionDto>
        {
            Items = items,
            PageNumber = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        };
    }

    public async Task<ZeroScoreDetailDto> GetZeroScoreDetailAsync(Guid id)
    {
        var violation = await _context.Violations
            .Include(v => v.Submission)
            .ThenInclude(s => s!.Exam)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (violation == null) throw new KeyNotFoundException("Violation not found");

        return new ZeroScoreDetailDto(
            violation.Id,
            violation.SubmissionId,
            violation.Submission!.StudentCode,
            violation.Submission!.Exam!.Name,
            violation.Submission.Score,
            violation.Submission.GradingComments,
            violation.ViolationType.ToString(),
            violation.Description,
            null, // Evidence path - could be added later
            violation.CreatedAt
        );
    }

    public async Task VerifyZeroScoreAsync(Guid id, VerifyZeroScoreRequest request, Guid moderatorId)
    {
        var violation = await _context.Violations.FindAsync(id);
        if (violation == null) throw new KeyNotFoundException("Violation not found");

        if (!violation.SubmissionId.HasValue)
            throw new KeyNotFoundException("Submission not found");

        var submission = await _context.Submissions.FindAsync(violation.SubmissionId.Value);
        if (submission == null) throw new KeyNotFoundException("Submission not found");

        if (request.Action == "confirm")
        {
            violation.Status = ViolationStatus.Resolved;
            // Keep score as 0
        }
        else if (request.Action == "override")
        {
            if (!request.OverrideScore.HasValue)
                throw new ArgumentException("Override score is required");

            violation.Status = ViolationStatus.Resolved;
            submission.Score = request.OverrideScore.Value;
            submission.ModeratorComments = request.ModeratorComment;
        }
        else
        {
            throw new ArgumentException("Invalid action");
        }

        await _context.SaveChangesAsync();
    }
}