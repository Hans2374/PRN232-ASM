using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Services.DTOs;
using PRN232_FA25_Assignment_G7.Services.Interfaces;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace PRN232_FA25_Assignment_G7.Services.Implementations
{
    public class ImportService : IImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImportService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<Guid, ImportJobContext> _activeJobs = new();

        private readonly string _storageRoot;
        private readonly string _filenamePattern;
        private readonly long _maxFileSize;

        public ImportService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ImportService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;

            _storageRoot = _configuration["Storage:RootPath"] ?? "storage";
            // Default pattern: permissive student folder/file names commonly used by students.
            // - `studentName`: 1-30 characters, letters, digits, underscores or hyphens
            // - `studentCode`: 1-8 letters (exam/code) followed by exactly 6 digits
            // This matches examples like: "anlqse150619", "AnhNSE171217", "DuongNMSE181515",
            // and variations in case. File extensions .doc/.docx/.pdf are optionally allowed.
            _filenamePattern = _configuration["Import:FilenamePattern"] ?? @"(?i)^(?<studentName>[A-Za-z0-9_\-]{1,30})(?<studentCode>[A-Za-z]{1,8}[0-9]{6})(?:\.(docx?|pdf))?$";
            _maxFileSize = long.Parse(_configuration["Import:MaxFileSize"] ?? "10485760"); // 10MB default
        }

        public async Task<Guid> StartImportJobAsync(Stream fileStream, string fileName, Guid subjectId, Guid semesterId, Guid examId, string initiatedBy)
        {
            // Validate inputs
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject == null) throw new ArgumentException("Subject not found");

            var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
            if (semester == null) throw new ArgumentException("Semester not found");

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null) throw new ArgumentException("Exam not found");

            // Create import job
            var jobId = Guid.NewGuid();
            // Resolve uploader user id from provided initiator value (could be user id GUID or username)
            User? uploader = null;
            if (Guid.TryParse(initiatedBy, out var initiatorGuid))
            {
                uploader = await _context.Users.FirstOrDefaultAsync(u => u.Id == initiatorGuid);
            }

            if (uploader == null)
            {
                uploader = await _context.Users.FirstOrDefaultAsync(u => u.Username == initiatedBy);
            }

            if (uploader == null)
            {
                throw new KeyNotFoundException($"Uploader user '{initiatedBy}' not found");
            }

            var job = new ImportJob
            {
                Id = jobId,
                ArchiveName = fileName,
                SubjectCode = subject.Code,
                SemesterCode = semester.Name,
                ExamId = exam.Id,
                UploaderUserId = uploader.Id,
                Status = ImportJobStatus.Pending,
                TotalFiles = 0,
                ProcessedFiles = 0,
                SuccessCount = 0,
                FailedCount = 0,
                ViolationsCount = 0,
                StartedAt = DateTime.UtcNow,
                StorageFolderPath = Path.Combine(_storageRoot, subject.Code, semester.Name, Path.GetFileNameWithoutExtension(fileName))
            };

            await _context.ImportJobs.AddAsync(job);
            await _context.SaveChangesAsync();

            // Start background processing using the provided stream (legacy path)
            var context = new ImportJobContext(jobId, fileStream, fileName, subject, semester, exam);
            _activeJobs[jobId] = context;

            _ = Task.Run(() => ProcessImportJobAsync(context));

            return jobId;
        }

        public async Task<Guid> StartImportJobAsync(string filePath, string fileName, Guid subjectId, Guid semesterId, Guid examId, string initiatedBy)
        {
            // Validate inputs (reuse logic from Stream-based overload)
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject == null) throw new ArgumentException("Subject not found");

            var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
            if (semester == null) throw new ArgumentException("Semester not found");

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null) throw new ArgumentException("Exam not found");

            // Create import job
            var jobId = Guid.NewGuid();
            // Resolve uploader user id from provided initiator value (could be user id GUID or username)
            User? uploader = null;
            if (Guid.TryParse(initiatedBy, out var initiatorGuid))
            {
                uploader = await _context.Users.FirstOrDefaultAsync(u => u.Id == initiatorGuid);
            }

            if (uploader == null)
            {
                uploader = await _context.Users.FirstOrDefaultAsync(u => u.Username == initiatedBy);
            }

            if (uploader == null)
            {
                throw new KeyNotFoundException($"Uploader user '{initiatedBy}' not found");
            }

            var job = new ImportJob
            {
                Id = jobId,
                ArchiveName = fileName,
                SubjectCode = subject.Code,
                SemesterCode = semester.Name,
                ExamId = exam.Id,
                UploaderUserId = uploader.Id,
                Status = ImportJobStatus.Pending,
                TotalFiles = 0,
                ProcessedFiles = 0,
                SuccessCount = 0,
                FailedCount = 0,
                ViolationsCount = 0,
                StartedAt = DateTime.UtcNow,
                StorageFolderPath = Path.Combine(_storageRoot, subject.Code, semester.Name, Path.GetFileNameWithoutExtension(fileName))
            };

            await _context.ImportJobs.AddAsync(job);
            await _context.SaveChangesAsync();

            // Start background processing using the saved file path
            var context = new ImportJobContext(jobId, filePath, fileName, subject, semester, exam);
            _activeJobs[jobId] = context;

            _ = Task.Run(() => ProcessImportJobAsync(context));

            return jobId;
        }

        public async Task<ImportStatusDto> GetImportStatusAsync(Guid jobId)
        {
            var job = await _context.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) throw new KeyNotFoundException("Import job not found");

            var context = _activeJobs.TryGetValue(jobId, out var ctx) ? ctx : null;

            return new ImportStatusDto
            {
                JobId = job.Id,
                Status = job.Status,
                StatusDescription = GetStatusDescription(job.Status),
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                TotalFiles = job.TotalFiles,
                ProcessedFiles = job.ProcessedFiles,
                SuccessCount = job.SuccessCount,
                FailedCount = job.FailedCount,
                ViolationsCount = job.ViolationsCount,
                ErrorMessage = job.ErrorMessage,
                ProgressMessages = context?.ProgressMessages.ToList() ?? new List<string>()
            };
        }

        public async Task<ImportResultsDto> GetImportResultsAsync(Guid jobId)
        {
            var job = await _context.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) throw new KeyNotFoundException("Import job not found");

            if (job.Status != ImportJobStatus.Completed)
                throw new InvalidOperationException("Import job is not completed yet");

            // Get successful imports
            var successfulImports = await _context.Submissions
                .Where(s => s.ImportJobId == jobId)
                .Select(s => new ImportResultItemDto
                {
                    SubmissionId = s.Id,
                    StudentCode = s.StudentCode,
                    FileName = s.OriginalFileName ?? string.Empty,
                    Score = s.Score,
                    ImportedAt = s.CreatedAt
                })
                .ToListAsync();

            // Get failed imports from job context or log
            var failedImports = new List<ImportFailureDto>();
            // Note: In a real implementation, you'd store failed imports in a separate table

            // Get duplicate groups
            var duplicateGroups = await _context.DuplicateGroups
                .Where(dg => dg.ExamId == job.ExamId)
                .Select(dg => new DuplicateGroupDto
                {
                    GroupId = dg.Id,
                    SimilarityScore = dg.SimilarityScore,
                    Submissions = dg.Submissions.Select(s => new DuplicateSubmissionDto
                    {
                        SubmissionId = s.Id,
                        StudentCode = s.StudentCode,
                        FileName = s.OriginalFileName ?? string.Empty
                    }).ToList()
                })
                .ToListAsync();

            // Get violations created during import
            var violations = await _context.Violations
                .Where(v => v.CreatedAt >= job.StartedAt && v.CreatedAt <= (job.CompletedAt ?? DateTime.UtcNow))
                .Select(v => new ViolationDto(
                    v.Id,
                    v.ViolationType.ToString(),
                    v.Description,
                    v.CreatedAt
                ))
                .ToListAsync();

            return new ImportResultsDto
            {
                JobId = job.Id,
                Status = job.Status,
                SuccessfulImports = successfulImports,
                FailedImports = failedImports,
                DuplicateGroups = duplicateGroups,
                Violations = violations,
                Summary = new ImportSummaryDto
                {
                    TotalFiles = job.TotalFiles,
                    SuccessfulImports = successfulImports.Count,
                    FailedImports = failedImports.Count,
                    DuplicatesFound = duplicateGroups.Count,
                    ViolationsCreated = violations.Count,
                    ProcessingTime = (job.CompletedAt ?? DateTime.UtcNow) - job.StartedAt
                }
            };
        }

        public async Task<ImportJobsListDto> GetImportJobsAsync(int page, int pageSize)
        {
            var query = _context.ImportJobs
                .OrderByDescending(j => j.StartedAt);

            var totalCount = await query.CountAsync();
            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var jobDtos = jobs.Select(j => new ImportJobSummaryDto
            {
                JobId = j.Id,
                Status = j.Status,
                ArchiveName = j.ArchiveName,
                SubjectCode = j.SubjectCode,
                SemesterCode = j.SemesterCode,
                StartedAt = j.StartedAt,
                CompletedAt = j.CompletedAt,
                TotalFiles = j.TotalFiles,
                SuccessCount = j.SuccessCount,
                FailedCount = j.FailedCount
            }).ToList();

            return new ImportJobsListDto
            {
                Jobs = jobDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task CancelImportJobAsync(Guid jobId)
        {
            var job = await _context.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) throw new KeyNotFoundException("Import job not found");

            if (job.Status != ImportJobStatus.Pending && job.Status != ImportJobStatus.Running)
                throw new InvalidOperationException("Job cannot be cancelled at this stage");

            job.Status = ImportJobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = "Job cancelled by user";

            await _context.SaveChangesAsync();

            // Remove from active jobs
            _activeJobs.TryRemove(jobId, out _);
        }

        private async Task ProcessImportJobAsync(ImportJobContext context)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await UpdateJobStatusAsync(db, context.JobId, ImportJobStatus.Running);
                context.AddProgressMessage("Starting import job processing");

                // Extract RAR archive. Prefer opening the saved file path inside the background task so
                // the stream is not tied to the HTTP request lifetime.
                List<ExtractedFileInfo> extractedFiles;
                if (!string.IsNullOrEmpty(context.FilePath))
                {
                    using var fsForArchive = File.OpenRead(context.FilePath);
                    extractedFiles = await ExtractRarArchiveAsync(fsForArchive, context.JobId);
                }
                else if (context.FileStream != null)
                {
                    extractedFiles = await ExtractRarArchiveAsync(context.FileStream, context.JobId);
                }
                else
                {
                    throw new InvalidOperationException("No archive source provided for import job");
                }
                context.AddProgressMessage($"Extracted {extractedFiles.Count} files from archive");

                // Update total files count
                await UpdateJobTotalFilesAsync(db, context.JobId, extractedFiles.Count);

                // Process each file
                var duplicateDetector = new DuplicateDetector();
                var processedCount = 0;

                foreach (var fileInfo in extractedFiles)
                {
                    if (context.IsCancelled) break;

                    try
                    {
                        await ProcessSubmissionFileAsync(db, fileInfo, context, duplicateDetector);
                        processedCount++;
                        await UpdateJobProgressAsync(db, context.JobId, processedCount);
                        context.AddProgressMessage($"Processed file: {fileInfo.FileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to process file {fileInfo.FileName}");
                        await IncrementFailedImportsAsync(db, context.JobId);
                        context.AddProgressMessage($"Failed to process file {fileInfo.FileName}: {ex.Message}");
                    }
                }

                // Perform duplicate detection
                if (!context.IsCancelled)
                {
                    await PerformDuplicateDetectionAsync(db, context, duplicateDetector);
                }

                // Complete job
                if (context.IsCancelled)
                {
                    await UpdateJobStatusAsync(db, context.JobId, ImportJobStatus.Failed);
                }
                else
                {
                    await UpdateJobStatusAsync(db, context.JobId, ImportJobStatus.Completed);
                    context.AddProgressMessage("Import job completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Import job {context.JobId} failed");
                // best-effort: create a scope and update status if possible
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await UpdateJobStatusAsync(db, context.JobId, ImportJobStatus.Failed, ex.Message);
                }
                catch { }

                context.AddProgressMessage($"Import job failed: {ex.Message}");
            }
            finally
            {
                _activeJobs.TryRemove(context.JobId, out _);
                    // If we used a temporary uploaded file, attempt to delete it to avoid filling temp storage.
                    try
                    {
                        if (!string.IsNullOrEmpty(context.FilePath) && File.Exists(context.FilePath))
                        {
                            File.Delete(context.FilePath);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup failures
                    }
            }
        }

        private async Task<List<ExtractedFileInfo>> ExtractRarArchiveAsync(Stream fileStream, Guid jobId)
        {
            var extractedFiles = new List<ExtractedFileInfo>();
            var tempDir = Path.Combine(Path.GetTempPath(), $"import_{jobId}");
            try
            {
                Directory.CreateDirectory(tempDir);
                // SharpCompress requires a seekable stream for some archive types (including RAR).
                // The incoming request stream may not be seekable, so copy to a temp file first.
                var archivePath = Path.Combine(tempDir, "archive.rar");
                using (var outFs = File.Create(archivePath))
                {
                    await fileStream.CopyToAsync(outFs);
                }

                using var fs = File.OpenRead(archivePath);
                using var archive = RarArchive.Open(fs);

                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    // Preserve directory structure inside the temp extraction folder so we can
                    // use parent folder names (student folders) for matching.
                    var entryKey = entry.Key ?? string.Empty;
                    var relativePath = entryKey.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                    var fileName = Path.GetFileName(relativePath);
                    if (string.IsNullOrEmpty(fileName)) continue;
                    var filePath = Path.Combine(tempDir, relativePath);

                    var dir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    using var entryStream = entry.OpenEntryStream();
                    using var outputStream = File.Create(filePath);
                    await entryStream.CopyToAsync(outputStream);

                    var parentFolder = string.Empty;
                    try
                    {
                        var parentDir = Path.GetDirectoryName(filePath);
                        if (!string.IsNullOrEmpty(parentDir) && !string.Equals(parentDir, tempDir, StringComparison.OrdinalIgnoreCase))
                        {
                            parentFolder = Path.GetFileName(parentDir) ?? string.Empty;
                        }
                    }
                    catch { }

                    extractedFiles.Add(new ExtractedFileInfo
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        Size = entry.Size,
                        ParentFolderName = parentFolder
                    });
                }
            }
            catch
            {
                // Cleanup on failure
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                throw;
            }

            return extractedFiles;
        }

        private async Task ProcessSubmissionFileAsync(ApplicationDbContext db, ExtractedFileInfo fileInfo, ImportJobContext context, DuplicateDetector duplicateDetector)
        {
            // Validate filename. First try the actual file name, then try the parent folder name (student folder).
            var match = Regex.Match(fileInfo.FileName, _filenamePattern);
            if (!match.Success)
            {
                if (!string.IsNullOrEmpty(fileInfo.ParentFolderName))
                {
                    match = Regex.Match(fileInfo.ParentFolderName, _filenamePattern);
                }
            }

            if (!match.Success)
            {
                await CreateViolationAsync(db, context.JobId, fileInfo.FileName, ViolationType.InvalidFormat,
                    $"Filename or parent folder does not match required pattern: {_filenamePattern}");
                return;
            }

            var studentCode = match.Groups["studentCode"].Value;

            // Check file size
            if (fileInfo.Size > _maxFileSize)
            {
                await CreateViolationAsync(db, context.JobId, fileInfo.FileName, ViolationType.InvalidFormat,
                    $"File size {fileInfo.Size} exceeds maximum allowed size {_maxFileSize}");
                return;
            }

            // Check for existing submission
            var existingSubmission = await db.Submissions
                .FirstOrDefaultAsync(s => s.ExamId == context.Exam.Id && s.StudentCode == studentCode);

            if (existingSubmission != null)
            {
                await CreateViolationAsync(db, context.JobId, fileInfo.FileName, ViolationType.Duplicate,
                    $"Student {studentCode} already has a submission for this exam");
                return;
            }

            // Process document content for duplicate detection
            var content = await ExtractDocumentContentAsync(fileInfo.FilePath);
            duplicateDetector.AddSubmission(studentCode, fileInfo.FileName, content);

            // Create submission
            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                StudentCode = studentCode,
                ExamId = context.Exam.Id,
                OriginalFileName = fileInfo.FileName,
                ImportJobId = context.JobId,
                CreatedAt = DateTime.UtcNow,
                SubmissionStatus = SubmissionStatus.Pending,
                ReviewStatus = ReviewStatus.None
            };

            await db.Submissions.AddAsync(submission);

            // Extract and store images
            var images = await ExtractImagesFromDocumentAsync(fileInfo.FilePath, submission.Id);
            foreach (var image in images)
            {
                await db.SubmissionImages.AddAsync(image);
            }

            await db.SaveChangesAsync();
            await IncrementSuccessfulImportsAsync(db, context.JobId);
        }

        private async Task<string> ExtractDocumentContentAsync(string filePath)
        {
            // Basic text extraction - in real implementation, use OpenXml SDK for Word docs
            var extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".docx")
            {
                // Use OpenXml to extract text
                return await Task.FromResult("Extracted text from Word document");
            }
            else if (extension == ".pdf")
            {
                // Use PDF library to extract text
                return await Task.FromResult("Extracted text from PDF document");
            }

            return string.Empty;
        }

        private async Task<List<SubmissionImage>> ExtractImagesFromDocumentAsync(string filePath, Guid submissionId)
        {
            // Extract images from document - implementation depends on document type
            return new List<SubmissionImage>();
        }

        private async Task PerformDuplicateDetectionAsync(ApplicationDbContext db, ImportJobContext context, DuplicateDetector duplicateDetector)
        {
            var duplicates = duplicateDetector.FindDuplicates();

            foreach (var duplicateGroup in duplicates)
            {
                var group = new DuplicateGroup
                {
                    Id = Guid.NewGuid(),
                    ExamId = context.Exam.Id,
                    GroupName = $"Duplicate Group {duplicateGroup.GroupId}",
                    SimilarityScore = duplicateGroup.SimilarityScore,
                    CreatedAt = DateTime.UtcNow
                };

                await db.DuplicateGroups.AddAsync(group);

                // Associate submissions with the group
                foreach (var sub in duplicateGroup.Submissions)
                {
                    var submission = await db.Submissions
                        .FirstOrDefaultAsync(s => s.StudentCode == sub.StudentCode && s.ExamId == context.Exam.Id);

                    if (submission != null)
                    {
                        // Note: This requires a many-to-many relationship or additional logic
                        // For now, we'll just create the group
                    }
                }

                await IncrementDuplicatesFoundAsync(db, context.JobId);
            }

            await db.SaveChangesAsync();
        }

        private async Task CreateViolationAsync(ApplicationDbContext db, Guid jobId, string fileName, ViolationType type, string description)
        {
            // Try to associate the violation with the uploader (job creator). If not available, leave CreatedBy as Guid.Empty
            var job = await db.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            var createdBy = job?.UploaderUserId ?? Guid.Empty;

            var violation = new Violation
            {
                Id = Guid.NewGuid(),
                SubmissionId = null, // Optional: some violations may not be tied to a saved submission
                ViolationType = type,
                Description = description,
                Severity = type == ViolationType.InvalidFormat ? ViolationSeverity.High : ViolationSeverity.Medium,
                Status = ViolationStatus.New,
                Evidence = new List<string> { fileName },
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            // If CreatedBy is Guid.Empty, attempt to set it to a fallback system user if such exists
            if (violation.CreatedBy == Guid.Empty)
            {
                var systemUser = await db.Users.FirstOrDefaultAsync(u => u.Username == "system" || u.Email == "system@localhost");
                if (systemUser != null)
                    violation.CreatedBy = systemUser.Id;
            }

            await db.Violations.AddAsync(violation);
            await db.SaveChangesAsync();
            await IncrementViolationsCreatedAsync(db, jobId);
        }

        #region Job Status Update Methods

        private async Task UpdateJobStatusAsync(ApplicationDbContext db, Guid jobId, ImportJobStatus status, string? errorMessage = null)
        {
            var job = await db.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job != null)
            {
                job.Status = status;
                if (status == ImportJobStatus.Running && job.StartedAt == default)
                    job.StartedAt = DateTime.UtcNow;
                if ((status == ImportJobStatus.Completed || status == ImportJobStatus.Failed) && !job.CompletedAt.HasValue)
                    job.CompletedAt = DateTime.UtcNow;
                job.ErrorMessage = errorMessage;
                await db.SaveChangesAsync();
            }
        }

        private async Task UpdateJobTotalFilesAsync(ApplicationDbContext db, Guid jobId, int totalFiles)
        {
            var job = await db.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job != null)
            {
                job.TotalFiles = totalFiles;
                await db.SaveChangesAsync();
            }
        }

        private async Task UpdateJobProgressAsync(ApplicationDbContext db, Guid jobId, int processedFiles)
        {
            var job = await db.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job != null)
            {
                job.ProcessedFiles = processedFiles;
                await db.SaveChangesAsync();
            }
        }

        private async Task IncrementSuccessfulImportsAsync(ApplicationDbContext db, Guid jobId)
        {
            var job = await db.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job != null)
            {
                job.SuccessCount++;
                await db.SaveChangesAsync();
            }
        }

        private async Task IncrementFailedImportsAsync(ApplicationDbContext db, Guid jobId)
        {
            var job = await db.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job != null)
            {
                job.FailedCount++;
                await db.SaveChangesAsync();
            }
        }

        private async Task IncrementDuplicatesFoundAsync(ApplicationDbContext db, Guid jobId)
        {
            // Note: ImportJob doesn't have a separate duplicates counter
            // Duplicates are tracked via DuplicateGroups
            await Task.CompletedTask;
        }

        private async Task IncrementViolationsCreatedAsync(ApplicationDbContext db, Guid jobId)
        {
            var job = await db.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job != null)
            {
                job.ViolationsCount++;
                await db.SaveChangesAsync();
            }
        }

        #endregion

        private string GetStatusDescription(ImportJobStatus status)
        {
            return status switch
            {
                ImportJobStatus.Pending => "Job is pending",
                ImportJobStatus.Running => "Job is currently being processed",
                ImportJobStatus.Completed => "Job completed successfully",
                ImportJobStatus.Failed => "Job failed with errors",
                _ => "Unknown status"
            };
        }
    }

    internal class ImportJobContext
    {
        public Guid JobId { get; }
        // Either FilePath (preferred) or FileStream (legacy) will be used by the background processor
        public string? FilePath { get; }
        public Stream? FileStream { get; }
        public string FileName { get; }
        public Subject Subject { get; }
        public Semester Semester { get; }
        public Exam Exam { get; }
        public bool IsCancelled { get; set; }
        public List<string> ProgressMessages { get; } = new();

        public ImportJobContext(Guid jobId, Stream fileStream, string fileName, Subject subject, Semester semester, Exam exam)
        {
            JobId = jobId;
            FileStream = fileStream;
            FileName = fileName;
            Subject = subject;
            Semester = semester;
            Exam = exam;
        }

        public ImportJobContext(Guid jobId, string filePath, string fileName, Subject subject, Semester semester, Exam exam)
        {
            JobId = jobId;
            FilePath = filePath;
            FileName = fileName;
            Subject = subject;
            Semester = semester;
            Exam = exam;
        }

        public void AddProgressMessage(string message)
        {
            ProgressMessages.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {message}");
        }
    }

    internal class ExtractedFileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ParentFolderName { get; set; } = string.Empty;
    }

    internal class DuplicateDetector
    {
        private readonly List<SubmissionContent> _submissions = new();

        public void AddSubmission(string studentCode, string fileName, string content)
        {
            _submissions.Add(new SubmissionContent
            {
                StudentCode = studentCode,
                FileName = fileName,
                Content = content,
                Tokens = Tokenize(content)
            });
        }

        public List<DuplicateGroupResult> FindDuplicates()
        {
            var results = new List<DuplicateGroupResult>();

            // Simple duplicate detection based on exact content match
            // In a real implementation, use MinHash or shingling for similarity
            var groupedByContent = _submissions.GroupBy(s => s.Content);

            foreach (var group in groupedByContent.Where(g => g.Count() > 1))
            {
                results.Add(new DuplicateGroupResult
                {
                    GroupId = Guid.NewGuid(),
                    SimilarityScore = 1.0m, // Exact match
                    Submissions = group.ToList()
                });
            }

            return results;
        }

        private List<string> Tokenize(string content)
        {
            // Simple tokenization - split by whitespace and remove punctuation
            return Regex.Split(content.ToLower(), @"\W+")
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .ToList();
        }
    }

    internal class SubmissionContent
    {
        public string StudentCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tokens { get; set; } = new();
    }

    internal class DuplicateGroupResult
    {
        public Guid GroupId { get; set; }
        public decimal SimilarityScore { get; set; }
        public List<SubmissionContent> Submissions { get; set; } = new();
    }
}