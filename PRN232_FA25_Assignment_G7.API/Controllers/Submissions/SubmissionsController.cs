using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.API.SignalR;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Services.Helpers;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using PRN232_FA25_Assignment_G7.Services.DTOs.Submission;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<SubmissionHub> _hubContext;
    private readonly ILogger<SubmissionsController> _logger;
    private readonly FileProcessingHelper _fileHelper;
    private readonly ViolationDetector _violationDetector;
    private readonly DuplicateChecker _duplicateChecker;
    private readonly WordImageExtractor _wordImageExtractor;
    private readonly IBulkUploadService _bulkUploadService;

    public SubmissionsController(
        ApplicationDbContext context,
        IHubContext<SubmissionHub> hubContext,
        ILogger<SubmissionsController> logger,
        FileProcessingHelper fileHelper,
        ViolationDetector violationDetector,
        DuplicateChecker duplicateChecker,
        WordImageExtractor wordImageExtractor,
        IBulkUploadService bulkUploadService)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
        _fileHelper = fileHelper;
        _violationDetector = violationDetector;
        _duplicateChecker = duplicateChecker;
        _wordImageExtractor = wordImageExtractor;
        _bulkUploadService = bulkUploadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse>> Get(Guid id, CancellationToken ct)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission == null) return NotFound();

        var response = new PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse(
            submission.Id,
            submission.ExamId,
            submission.Exam?.Name ?? string.Empty,
            submission.StudentCode,
            submission.OriginalFileName,
            submission.ExtractedFolderPath,
            submission.Score,
            submission.CreatedAt,
            submission.Violations.Count
        );

        return Ok(response);
    }

    [HttpGet("exam/{examId:guid}")]
    public async Task<ActionResult<IReadOnlyList<PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse>>> GetByExam(Guid examId, CancellationToken ct)
    {
        var submissions = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .Where(s => s.ExamId == examId)
            .ToListAsync(ct);

        var response = submissions.Select(s => new PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse(
            s.Id,
            s.ExamId,
            s.Exam?.Name ?? string.Empty,
            s.StudentCode,
            s.OriginalFileName,
            s.ExtractedFolderPath,
            s.Score,
            s.CreatedAt,
            s.Violations.Count
        )).ToList();

        return Ok(response);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Manager,Moderator,Examiner")]
    [RequestSizeLimit(100_000_000)] //100MB
    public async Task<ActionResult<PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse>> Upload([FromForm] PRN232_FA25_Assignment_G7.API.DTOs.ProcessSubmissionRequest request, CancellationToken ct)
    {
        var exam = await _context.Exams.FindAsync([request.ExamId], ct);
        if (exam == null) return BadRequest("Exam not found.");

        // Save uploaded file to disk under uploads/{examId}/{studentCode}/{timestamp}
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var baseUploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        var submissionFolder = Path.Combine(baseUploads, request.ExamId.ToString(), request.StudentCode, timestamp);
        Directory.CreateDirectory(submissionFolder);

        var savedFilePath = Path.Combine(submissionFolder, request.File.FileName);
        await using (var fs = new FileStream(savedFilePath, FileMode.Create))
        {
            await request.File.CopyToAsync(fs, ct).ConfigureAwait(false);
        }

        // Extract archive if needed and get extracted folder
        string extractedFolder = submissionFolder;
        try
        {
            extractedFolder = await _fileHelper.ExtractSubmissionAsync(savedFilePath, submissionFolder, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract submission: {File}", savedFilePath);
            // proceed with original folder
        }

        // Create submission record
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            StudentCode = request.StudentCode,
            OriginalFileName = request.File.FileName,
            ExtractedFolderPath = extractedFolder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync(ct);

        // Run violation detection
        var detected = await _violationDetector.ScanSubmissionAsync(extractedFolder, ct).ConfigureAwait(false);
        var violationEntities = new List<Violation>();
        foreach (var d in detected)
        {
            var ve = new Violation
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                Type = d.Type,
                Description = d.Description,
                Severity = d.Severity,
                IsZeroScore = d.IsZeroScore
            };
            violationEntities.Add(ve);
        }

        if (violationEntities.Any())
        {
            _context.Violations.AddRange(violationEntities);
            await _context.SaveChangesAsync(ct);

            // Broadcast violation flagged for each
            foreach (var v in violationEntities)
            {
                await _hubContext.Clients.All.SendAsync("ViolationFlagged", new
                {
                    SubmissionId = submission.Id,
                    StudentCode = submission.StudentCode,
                    ViolationType = v.Type,
                    IsZeroScore = v.IsZeroScore,
                    Timestamp = DateTime.UtcNow
                }, ct).ConfigureAwait(false);
            }
        }

        // Extract images from Word documents and save them
        var docxFiles = Directory.EnumerateFiles(extractedFolder, "*.docx", SearchOption.AllDirectories).ToList();
        var extractedImages = new List<string>();
        foreach (var docx in docxFiles)
        {
            var imgs = await _wordImageExtractor.ExtractImagesFromWordAsync(docx, Path.Combine(submissionFolder, "images"), ct).ConfigureAwait(false);
            extractedImages.AddRange(imgs);
        }

        // Run duplicate detection against other submissions in same exam
        var otherSubmissions = await _context.Submissions.Where(s => s.ExamId == request.ExamId && s.Id != submission.Id).ToListAsync(ct).ConfigureAwait(false);
        var duplicateReports = new List<object>();
        var myFiles = await _fileHelper.GetAllCodeFilesAsync(extractedFolder, ct).ConfigureAwait(false);

        foreach (var other in otherSubmissions)
        {
            var otherFolder = other.ExtractedFolderPath;
            if (string.IsNullOrEmpty(otherFolder) || !Directory.Exists(otherFolder)) continue;
            var otherFiles = await _fileHelper.GetAllCodeFilesAsync(otherFolder, ct).ConfigureAwait(false);
            double bestSim =0.0;
            string? myFilePath = null;
            string? otherFilePath = null;

            foreach (var a in myFiles)
            {
                foreach (var b in otherFiles)
                {
                    var sim = await _duplicateChecker.CalculateSimilarityAsync(a, b, ct).ConfigureAwait(false);
                    if (sim > bestSim)
                    {
                        bestSim = sim;
                        myFilePath = a;
                        otherFilePath = b;
                    }
                }
            }

            if (bestSim >=0.6) // threshold
            {
                // create violation
                var dupViolation = new Violation
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    Type = "Plagiarism",
                    Description = $"Similarity {bestSim:P0} with submission {other.Id} (files: {Path.GetFileName(myFilePath ?? "")} vs {Path.GetFileName(otherFilePath ?? "")})",
                    Severity = (int)Math.Round(bestSim *10),
                    IsZeroScore = bestSim >=0.9
                };
                _context.Violations.Add(dupViolation);
                await _context.SaveChangesAsync(ct).ConfigureAwait(false);

                duplicateReports.Add(new
                {
                    OtherSubmissionId = other.Id,
                    Similarity = bestSim,
                    MyFile = myFilePath,
                    OtherFile = otherFilePath
                });

                // Broadcast duplicate found
                await _hubContext.Clients.All.SendAsync("DuplicateFound", new
                {
                    SubmissionId = submission.Id,
                    OtherSubmissionId = other.Id,
                    Similarity = bestSim,
                    Timestamp = DateTime.UtcNow
                }, ct).ConfigureAwait(false);
            }
        }

        // Broadcast submission uploaded
        await _hubContext.Clients.All.SendAsync("SubmissionUploaded", new
        {
            SubmissionId = submission.Id,
            StudentCode = submission.StudentCode,
            ExamName = exam.Name,
            Timestamp = DateTime.UtcNow
        }, ct).ConfigureAwait(false);

        var response = new PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse(
            submission.Id,
            submission.ExamId,
            exam.Name,
            submission.StudentCode,
            submission.OriginalFileName,
            submission.ExtractedFolderPath,
            submission.Score,
            submission.CreatedAt,
            await _context.Violations.CountAsync(v => v.SubmissionId == submission.Id, ct).ConfigureAwait(false)
        );

        // Optionally include duplicateReports or extractedImages in a richer response via SignalR or separate endpoint.

        return CreatedAtAction(nameof(Get), new { id = submission.Id }, response);
    }

    [HttpPost("bulk-upload")]
    [Authorize(Roles = "Admin,Manager")]
    [RequestSizeLimit(500_000_000)] // 500MB for bulk uploads
    public async Task<ActionResult<BulkUploadResult>> BulkUpload([FromForm] Guid examId, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archive file is required");

        // Validate file extension
        var allowedExtensions = new[] { ".rar", ".zip", ".7z", ".tar", ".gz" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Only RAR, ZIP, 7Z, TAR, and GZ archives are supported");

        // Validate exam exists
        var exam = await _context.Exams.FindAsync([examId], ct);
        if (exam == null)
            return BadRequest("Exam not found");

        // Save the uploaded archive temporarily
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"bulk_upload_{Guid.NewGuid()}{extension}");
        try
        {
            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            // Create bulk upload request
            var bulkRequest = new BulkUploadRequest(examId, tempFilePath, file.FileName);

            // Create progress reporter
            var progress = new Progress<BulkUploadProgress>(p =>
            {
                _hubContext.Clients.All.SendAsync("BulkUploadProgress", new
                {
                    ExamId = examId,
                    Status = p.Status,
                    TotalFiles = p.TotalFiles,
                    ProcessedFiles = p.ProcessedFiles,
                    ViolationsFound = p.ViolationsFound,
                    DuplicatesFound = p.DuplicatesFound,
                    Errors = p.Errors
                }, ct).ConfigureAwait(false);
            });

            // Process bulk upload
            var result = await _bulkUploadService.ProcessBulkUploadAsync(bulkRequest, progress, ct);

            // Broadcast completion
            await _hubContext.Clients.All.SendAsync("BulkUploadCompleted", new
            {
                ExamId = examId,
                Result = result
            }, ct).ConfigureAwait(false);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk upload failed for exam {ExamId}", examId);
            return StatusCode(500, $"Bulk upload failed: {ex.Message}");
        }
        finally
        {
            // Cleanup temp file
            try
            {
                if (System.IO.File.Exists(tempFilePath)) System.IO.File.Delete(tempFilePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [HttpPost("{id:guid}/score")]
    [Authorize(Roles = "Admin,Manager,Moderator,Examiner")]
    public async Task<ActionResult<PRN232_FA25_Assignment_G7.API.DTOs.SubmissionScoringResult>> Score(Guid id, [FromBody] decimal score, CancellationToken ct)
    {
        var submission = await _context.Submissions
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission == null) return NotFound();

        var hasZeroScoreViolation = submission.Violations.Any(v => v.IsZeroScore);
        if (hasZeroScoreViolation)
        {
            submission.Score = 0;
            await _context.SaveChangesAsync(ct);

            await _hubContext.Clients.All.SendAsync("SubmissionGraded", new
            {
                SubmissionId = submission.Id,
                StudentCode = submission.StudentCode,
                Score = 0m,
                Timestamp = DateTime.UtcNow
            }, ct);

            return Ok(new PRN232_FA25_Assignment_G7.API.DTOs.SubmissionScoringResult(submission.Id, 0, true, "Zero score due to violation."));
        }

        submission.Score = score;
        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.All.SendAsync("SubmissionGraded", new
        {
            SubmissionId = submission.Id,
            StudentCode = submission.StudentCode,
            Score = score,
            Timestamp = DateTime.UtcNow
        }, ct);

        return Ok(new PRN232_FA25_Assignment_G7.API.DTOs.SubmissionScoringResult(submission.Id, score, false, string.Empty));
    }

    [HttpGet("{id:guid}/detail")]
    public async Task<ActionResult<SubmissionDetailResponse>> GetDetail(Guid id, CancellationToken ct)
    {
        var submission = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission == null) return NotFound();

        var response = new SubmissionDetailResponse(
            submission.Id,
            submission.ExamId,
            submission.Exam?.Name ?? string.Empty,
            submission.StudentCode,
            submission.OriginalFileName,
            submission.ExtractedFolderPath,
            submission.Score,
            submission.SecondScore,
            submission.CreatedAt,
            submission.SubmissionStatus.ToString(),
            submission.ReviewStatus.ToString(),
            submission.GradingComments,
            submission.SecondGradingComments,
            submission.ModeratorComments,
            submission.AdminComments,
            submission.GradedAt,
            submission.SecondGradedAt,
            submission.Violations.Count
        );

        return Ok(response);
    }

    [HttpPost("{id:guid}/grade")]
    [Authorize(Roles = "Examiner")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeSubmissionRequest request, CancellationToken ct)
    {
        var submission = await _context.Submissions.FindAsync([id], ct);
        if (submission == null) return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        submission.Score = request.Score;
        submission.GradingComments = request.Comments;
        submission.GradedBy = Guid.Parse(userId);
        submission.GradedAt = DateTime.UtcNow;
        submission.SubmissionStatus = SubmissionStatus.Graded;

        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.All.SendAsync("SubmissionGraded", new
        {
            SubmissionId = submission.Id,
            StudentCode = submission.StudentCode,
            Score = request.Score,
            Timestamp = DateTime.UtcNow
        }, ct);

        return Ok();
    }

    [HttpPost("{id:guid}/double-grade")]
    [Authorize(Roles = "Examiner")]
    public async Task<IActionResult> DoubleGrade(Guid id, [FromBody] DoubleGradeRequest request, CancellationToken ct)
    {
        var submission = await _context.Submissions.FindAsync([id], ct);
        if (submission == null) return NotFound();

        if (submission.SubmissionStatus != SubmissionStatus.Graded)
            return BadRequest("Submission must be graded first");

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (submission.GradedBy == Guid.Parse(userId))
            return BadRequest("Cannot double-grade your own submission");

        submission.SecondScore = request.SecondScore;
        submission.SecondGradingComments = request.Comments;
        submission.SecondGradedBy = Guid.Parse(userId);
        submission.SecondGradedAt = DateTime.UtcNow;
        submission.SubmissionStatus = SubmissionStatus.DoubleGraded;

        await _context.SaveChangesAsync(ct);

        return Ok();
    }

    [HttpPost("{id:guid}/moderator-adjust-score")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> ModeratorAdjustScore(Guid id, [FromBody] AdjustScoreRequest request, CancellationToken ct)
    {
        var submission = await _context.Submissions.FindAsync([id], ct);
        if (submission == null) return NotFound();

        submission.Score = request.NewScore;
        submission.ModeratorComments = request.Reason;
        submission.ReviewStatus = ReviewStatus.Completed;

        await _context.SaveChangesAsync(ct);

        return Ok();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyList<PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse>>> GetAll(CancellationToken ct)
    {
        var submissions = await _context.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Violations)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var response = submissions.Select(s => new PRN232_FA25_Assignment_G7.API.DTOs.SubmissionResponse(
            s.Id,
            s.ExamId,
            s.Exam?.Name ?? string.Empty,
            s.StudentCode,
            s.OriginalFileName,
            s.ExtractedFolderPath,
            s.Score,
            s.CreatedAt,
            s.Violations.Count
        )).ToList();

        return Ok(response);
    }
}
