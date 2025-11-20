using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;
using PRN232_FA25_Assignment_G7.Services.DTOs;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PRN232_FA25_Assignment_G7.Services.Implementations;

public class ExaminerService : IExaminerService
{
    private readonly ApplicationDbContext _context;

    public ExaminerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExaminerDashboardDto?> GetDashboardAsync(Guid examinerId, CancellationToken ct = default)
    {
        // Get assigned subjects
        var assignedSubjectIds = await _context.ExaminerSubjects
            .Where(es => es.ExaminerId == examinerId)
            .Select(es => es.SubjectId)
            .ToListAsync(ct);

        // Get exams for assigned subjects
        var exams = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.Submissions)
            .Where(e => assignedSubjectIds.Contains(e.SubjectId))
            .ToListAsync(ct);

        var assignedExams = exams.Select(e => new AssignedExamDto(
            e.Id,
            e.Name,
            e.Subject!.Name,
            e.ExamDate,
            e.Submissions.Count,
            e.Submissions.Count(s => s.GradedBy == examinerId || s.SecondGradedBy == examinerId),
            true // Assume primary grader for now
        )).ToList();

        var totalPending = exams.Sum(e => e.Submissions.Count(s => s.GradedBy == null));
        var totalGraded = exams.Sum(e => e.Submissions.Count(s => s.GradedBy == examinerId || s.SecondGradedBy == examinerId));

        var pendingDoubleGrading = await _context.Submissions
            .CountAsync(s => s.SecondGradedBy == examinerId && s.SecondGradedAt == null, ct);

        var recentActivities = await _context.Submissions
            .Where(s => s.GradedBy == examinerId || s.SecondGradedBy == examinerId)
            .OrderByDescending(s => s.GradedAt ?? s.SecondGradedAt)
            .Take(10)
            .Select(s => new RecentActivityDto(
                s.Id,
                s.GradedBy == examinerId ? "Graded" : "Double Graded",
                s.Score,
                s.GradedAt ?? s.SecondGradedAt ?? DateTime.UtcNow
            ))
            .ToListAsync(ct);

        return new ExaminerDashboardDto(
            assignedExams.Count,
            totalPending,
            totalGraded,
            pendingDoubleGrading,
            assignedExams,
            recentActivities
        );
    }

    public async Task<IReadOnlyList<ExaminerResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Examiners
            .Include(e => e.ExaminerSubjects)
            .ThenInclude(es => es.Subject)
            .Select(e => new ExaminerResponse(
                e.Id,
                e.FullName,
                e.Email,
                e.ExaminerSubjects.Select(es => es.Subject!.Name).ToList()
            ))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<SubmissionSummaryDto>> GetAssignedSubmissionsAsync(Guid examinerId, SubmissionFilterDto filter, CancellationToken ct = default)
    {
        var assignedSubjectIds = await _context.ExaminerSubjects
            .Where(es => es.ExaminerId == examinerId)
            .Select(es => es.SubjectId)
            .ToListAsync(ct);

        if (!assignedSubjectIds.Any())
            return new PagedResult<SubmissionSummaryDto>
            {
                Items = new List<SubmissionSummaryDto>(),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = 0,
                TotalPages = 0
            };

        var query = _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .Where(s => assignedSubjectIds.Contains(s.Exam!.SubjectId));

        if (filter.ExamId.HasValue)
        {
            query = query.Where(s => s.ExamId == filter.ExamId.Value);
        }

        switch (filter.Status.ToLower())
        {
            case "pending":
                query = query.Where(s => s.GradedBy == null);
                break;
            case "completed":
                query = query.Where(s => s.GradedBy == examinerId);
                break;
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var submissions = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var items = submissions.Select(s => new SubmissionSummaryDto(
            s.Id,
            s.ExamId,
            s.Exam?.Name ?? string.Empty,
            s.StudentCode,
            s.StudentCode,
            s.OriginalFileName,
            s.Score,
            s.CreatedAt,
            s.Violations.Count
        )).ToList();

        return new PagedResult<SubmissionSummaryDto>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<SubmissionDetailDto?> GetSubmissionDetailAsync(Guid submissionId, Guid examinerId, CancellationToken ct = default)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct);

        if (submission == null) return null;

        var isAssigned = await _context.ExaminerSubjects
            .AnyAsync(es => es.SubjectId == submission.Exam!.SubjectId && es.ExaminerId == examinerId, ct);

        if (!isAssigned) return null;

        var rubric = submission.Exam?.Rubrics.Select(r => new RubricCriterionDto(
            r.Id,
            r.Criteria,
            r.MaxScore
        )).ToList() ?? new List<RubricCriterionDto>();

        var scores = new List<ScoreDto>(); // No individual scores stored

        var violations = submission.Violations.Select(v => new ViolationDto(
            v.Id,
            v.Type,
            v.Description,
            v.CreatedAt
        )).ToList();

        return new SubmissionDetailDto(
            submission.Id,
            submission.StudentCode,
            submission.StudentCode,
            submission.ExamId,
            submission.Exam?.Name ?? string.Empty,
            $"/api/files/submissions/{submission.Id}",
            violations,
            rubric,
            scores,
            submission.Score,
            submission.GradingComments,
            submission.CreatedAt
        );
    }

    public async Task<bool> SubmitGradeAsync(Guid submissionId, SubmitGradeDto dto, Guid examinerId, CancellationToken ct = default)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct);

        if (submission == null || submission.GradedBy.HasValue) return false;

        var totalScore = dto.Scores.Sum(s => s.Score);
        var maxScore = submission.Exam?.Rubrics.Sum(r => r.MaxScore) ?? 0;

        if (totalScore > maxScore) return false;

        // No individual scores stored - just total score
        submission.Score = totalScore;
        submission.GradedBy = examinerId;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradingComments = dto.FinalComment;
        submission.SubmissionStatus = Repositories.Entities.SubmissionStatus.Graded;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PagedResult<DoubleGradingTaskDto>> GetDoubleGradingTasksAsync(Guid examinerId, DoubleGradingFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Submissions
            .Include(s => s.Exam)
            .Where(s => s.SecondGradedBy == examinerId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var submissions = await query
            .OrderByDescending(s => s.SecondGradedAt ?? s.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var items = submissions.Select(s => new DoubleGradingTaskDto(
            s.Id,
            s.StudentCode,
            s.StudentCode,
            s.Exam?.Name ?? string.Empty,
            s.Score,
            s.SecondScore,
            s.SecondGradedAt ?? s.CreatedAt
        )).ToList();

        return new PagedResult<DoubleGradingTaskDto>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<DoubleGradingDetailDto?> GetDoubleGradingDetailAsync(Guid submissionId, Guid examinerId, CancellationToken ct = default)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.SecondGradedBy == examinerId, ct);

        if (submission == null) return null;

        var rubric = submission.Exam?.Rubrics.Select(r => new RubricCriterionDto(
            r.Id,
            r.Criteria,
            r.MaxScore
        )).ToList() ?? new List<RubricCriterionDto>();

        // No individual scores stored - simplified
        var scores = new List<ScoreDto>();
        var primaryScores = new List<ScoreDto>();

        var violations = submission.Violations.Select(v => new ViolationDto(
            v.Id,
            v.Type,
            v.Description,
            v.CreatedAt
        )).ToList();

        return new DoubleGradingDetailDto(
            submission.Id,
            submission.StudentCode,
            submission.StudentCode,
            submission.ExamId,
            submission.Exam?.Name ?? string.Empty,
            $"/api/files/submissions/{submission.Id}",
            violations,
            rubric,
            scores,
            primaryScores,
            submission.Score,
            submission.SecondScore,
            submission.SecondGradingComments,
            submission.CreatedAt,
            submission.SecondGradedAt ?? submission.CreatedAt
        );
    }

    public async Task<bool> SubmitDoubleGradeAsync(Guid submissionId, SubmitDoubleGradingDto dto, Guid examinerId, CancellationToken ct = default)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .ThenInclude(e => e!.Rubrics)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.SecondGradedBy == examinerId, ct);

        if (submission == null || submission.SecondGradedAt.HasValue) return false;

        var totalScore = dto.Scores.Sum(s => s.Score);
        var maxScore = submission.Exam?.Rubrics.Sum(r => r.MaxScore) ?? 0;

        if (totalScore > maxScore) return false;

        // No individual scores stored - just total score
        submission.SecondScore = totalScore;
        submission.SecondGradedAt = DateTime.UtcNow;
        submission.SecondGradingComments = dto.FinalComment;
        submission.SubmissionStatus = Repositories.Entities.SubmissionStatus.DoubleGraded;
        submission.SubmissionStatus = Repositories.Entities.SubmissionStatus.DoubleGraded;

        var primaryScore = submission.Score ?? 0;
        var difference = Math.Abs(primaryScore - totalScore);

        if (difference > 10)
        {
            var complaint = new Repositories.Entities.Complaint
            {
                SubmissionId = submissionId,
                Title = "Score Discrepancy",
                Description = $"Score discrepancy detected. Primary: {primaryScore}, Secondary: {totalScore}, Difference: {difference}",
                Status = Repositories.Entities.ComplaintStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Complaints.Add(complaint);
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }
}
