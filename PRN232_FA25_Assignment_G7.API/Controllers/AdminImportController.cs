using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PRN232_FA25_Assignment_G7.API.Controllers
{
    [ApiController]
    [Route("api/admin/import")]
    [Authorize(Roles = "Admin")]
    public class AdminImportController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminImportController> _logger;

        public AdminImportController(IImportService importService, IConfiguration configuration, ILogger<AdminImportController> logger)
        {
            _importService = importService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Upload and process a RAR archive containing student submissions
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadRarArchive([FromForm] ImportRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded");

            // Check file size against configured limit
            var maxUploadBytes = _configuration.GetValue<long>("UploadLimits:MaxUploadBytes", 1073741824); // Default 1GB
            if (request.File.Length > maxUploadBytes)
                return BadRequest($"RAR file too large. Maximum allowed size is {maxUploadBytes / (1024 * 1024 * 1024):F1}GB.");

            // Log upload attempt
            _logger.LogInformation("RAR upload started: {FileName}, Size: {Size} bytes ({SizeMB:F2} MB)",
                request.File.FileName, request.File.Length, request.File.Length / (1024.0 * 1024.0));

            if (!request.File.FileName.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only RAR files are supported");

            try
            {
                // Determine initiator: prefer NameIdentifier (GUID), then JWT sub, then username
                var initiator = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                               ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(initiator))
                    return Unauthorized(new { error = "Unable to determine uploader identity from token" });

                // Save uploaded file to a temp file while the request is active so background processing
                // can open the file later without depending on the request stream lifetime.
                var tempFileName = $"upload_{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
                using (var outFs = System.IO.File.Create(tempFilePath))
                using (var uploadStream = request.File.OpenReadStream())
                {
                    await uploadStream.CopyToAsync(outFs);
                }

                var jobId = await _importService.StartImportJobAsync(
                    tempFilePath,
                    request.File.FileName,
                    request.SubjectId,
                    request.SemesterId,
                    request.ExamId,
                    initiator
                );

                return Ok(new { jobId, message = "Import job started successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get the status of an import job
        /// </summary>
        [HttpGet("status/{jobId}")]
        public async Task<IActionResult> GetImportStatus(Guid jobId)
        {
            try
            {
                var status = await _importService.GetImportStatusAsync(jobId);
                var response = new ImportStatusResponse
                {
                    JobId = status.JobId,
                    Status = status.Status,
                    StatusDescription = status.StatusDescription,
                    StartedAt = status.StartedAt,
                    CompletedAt = status.CompletedAt,
                    TotalFiles = status.TotalFiles,
                    ProcessedFiles = status.ProcessedFiles,
                    SuccessCount = status.SuccessCount,
                    FailedCount = status.FailedCount,
                    ViolationsCount = status.ViolationsCount,
                    ErrorMessage = status.ErrorMessage,
                    ProgressMessages = status.ProgressMessages
                };
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Import job not found");
            }
        }

        /// <summary>
        /// Get the results of a completed import job
        /// </summary>
        [HttpGet("results/{jobId}")]
        public async Task<IActionResult> GetImportResults(Guid jobId)
        {
            try
            {
                var results = await _importService.GetImportResultsAsync(jobId);
                var response = new ImportResultsResponse
                {
                    JobId = results.JobId,
                    Status = results.Status,
                    SuccessfulImports = results.SuccessfulImports.Select(s => new ImportResultItemResponse
                    {
                        SubmissionId = s.SubmissionId,
                        StudentCode = s.StudentCode,
                        FileName = s.FileName,
                        Score = s.Score,
                        ImportedAt = s.ImportedAt
                    }).ToList(),
                    FailedImports = results.FailedImports.Select(f => new ImportFailureResponse
                    {
                        FileName = f.FileName,
                        ErrorMessage = f.ErrorMessage,
                        StudentCode = f.StudentCode
                    }).ToList(),
                    DuplicateGroups = results.DuplicateGroups.Select(d => new DuplicateGroupResponse
                    {
                        GroupId = d.GroupId,
                        SimilarityScore = d.SimilarityScore,
                        Submissions = d.Submissions.Select(s => new DuplicateSubmissionResponse
                        {
                            SubmissionId = s.SubmissionId,
                            StudentCode = s.StudentCode,
                            FileName = s.FileName
                        }).ToList()
                    }).ToList(),
                    Violations = results.Violations.Select(v => new ViolationResponse(v.Id, Guid.Empty, string.Empty, v.Type, v.Description, ViolationSeverity.Low, ViolationStatus.New)).ToList(),
                    Summary = new ImportSummaryResponse
                    {
                        TotalFiles = results.Summary.TotalFiles,
                        SuccessfulImports = results.Summary.SuccessfulImports,
                        FailedImports = results.Summary.FailedImports,
                        DuplicatesFound = results.Summary.DuplicatesFound,
                        ViolationsCreated = results.Summary.ViolationsCreated,
                        ProcessingTime = results.Summary.ProcessingTime
                    }
                };
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Import job not found");
            }
        }

        /// <summary>
        /// Get all import jobs with pagination
        /// </summary>
        [HttpGet("jobs")]
        public async Task<IActionResult> GetImportJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var jobs = await _importService.GetImportJobsAsync(page, pageSize);
            var response = new ImportJobsListResponse
            {
                Jobs = jobs.Jobs.Select(j => new ImportJobSummaryResponse
                {
                    JobId = j.JobId,
                    Status = j.Status,
                    ArchiveName = j.ArchiveName,
                    SubjectCode = j.SubjectCode,
                    SemesterCode = j.SemesterCode,
                    StartedAt = j.StartedAt,
                    CompletedAt = j.CompletedAt,
                    TotalFiles = j.TotalFiles,
                    SuccessCount = j.SuccessCount,
                    FailedCount = j.FailedCount
                }).ToList(),
                TotalCount = jobs.TotalCount,
                Page = jobs.Page,
                PageSize = jobs.PageSize,
                TotalPages = jobs.TotalPages
            };
            return Ok(response);
        }

        /// <summary>
        /// Cancel an import job
        /// </summary>
        [HttpPost("cancel/{jobId}")]
        public async Task<IActionResult> CancelImportJob(Guid jobId)
        {
            try
            {
                await _importService.CancelImportJobAsync(jobId);
                return Ok(new { message = "Import job cancelled successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Import job not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}