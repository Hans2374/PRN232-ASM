using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FA25_Assignment_G7.RazorPages.Services;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Exams
{
    [Authorize(Roles = "Admin")]
    public class AssignExaminersModel : PageModel
    {
        private readonly ApiClient _apiClient;

        public AssignExaminersModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public ExamDetailInfo Exam { get; set; } = default!;
        public List<AssignedExaminer> AssignedExaminers { get; set; } = new();
        public List<SelectListItem> AvailableExaminers { get; set; } = new();

        [BindProperty]
        public AssignInput Assignment { get; set; } = default!;

        public class AssignInput
        {
            public int ExamId { get; set; }
            public int ExaminerId { get; set; }
            public bool IsPrimaryGrader { get; set; }
        }

        public class ExamDetailInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public string SemesterName { get; set; } = string.Empty;
        }

        public class AssignedExaminer
        {
            public int ExaminerId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public bool IsPrimaryGrader { get; set; }
            public DateTime AssignedAt { get; set; }
        }

        public class ExaminerInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // Get exam details
                var examDetail = await _apiClient.GetAsync<ExamDetailResponse>($"/api/exams/{id}");
                if (examDetail == null)
                {
                    TempData["Error"] = "Exam not found";
                    return RedirectToPage("./Index");
                }

                Exam = new ExamDetailInfo
                {
                    Id = examDetail.Id,
                    Name = examDetail.Name,
                    SubjectName = examDetail.Subject.Name,
                    SemesterName = examDetail.Semester.Name
                };

                // Get assigned examiners
                var assigned = await _apiClient.GetAsync<List<ExamExaminerDto>>($"/api/exams/{id}/examiners");
                if (assigned != null)
                {
                    AssignedExaminers = assigned.Select(e => new AssignedExaminer
                    {
                        ExaminerId = e.ExaminerId,
                        Name = e.ExaminerName,
                        Email = e.ExaminerEmail,
                        IsPrimaryGrader = e.IsPrimaryGrader,
                        AssignedAt = e.AssignedAt
                    }).ToList();
                }

                // Get all examiners
                var examiners = await _apiClient.GetAsync<List<ExaminerInfo>>("/api/examiners");
                if (examiners != null)
                {
                    // Filter out already assigned examiners
                    var assignedIds = AssignedExaminers.Select(e => e.ExaminerId).ToHashSet();
                    AvailableExaminers = examiners
                        .Where(e => !assignedIds.Contains(e.Id))
                        .Select(e => new SelectListItem
                        {
                            Value = e.Id.ToString(),
                            Text = $"{e.Name} ({e.Email})"
                        })
                        .ToList();
                }

                Assignment = new AssignInput { ExamId = id };
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading exam: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAssignAsync()
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync(Assignment.ExamId);
            }

            try
            {
                var request = new AssignRequest
                {
                    ExaminerId = Assignment.ExaminerId,
                    IsPrimaryGrader = Assignment.IsPrimaryGrader
                };

                await _apiClient.PostAsync<AssignRequest, object>($"/api/exams/{Assignment.ExamId}/assign", request);

                TempData["Success"] = "Examiner assigned successfully";
                return RedirectToPage(new { id = Assignment.ExamId });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex)
            {
                TempData["Error"] = $"Failed to assign examiner: {ex.Message}";
                return await OnGetAsync(Assignment.ExamId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error assigning examiner: {ex.Message}";
                return await OnGetAsync(Assignment.ExamId);
            }
        }

        public async Task<IActionResult> OnPostUnassignAsync(int examId, int examinerId)
        {
            try
            {
                await _apiClient.DeleteAsync($"/api/exams/{examId}/examiners/{examinerId}");

                TempData["Success"] = "Examiner unassigned successfully";

                return RedirectToPage(new { id = examId });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (ApiException ex)
            {
                TempData["Error"] = $"Failed to unassign examiner: {ex.Message}";
                return RedirectToPage(new { id = examId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error unassigning examiner: {ex.Message}";
                return RedirectToPage(new { id = examId });
            }
        }

        private class AssignRequest
        {
            public int ExaminerId { get; set; }
            public bool IsPrimaryGrader { get; set; }
        }

        private class ExamDetailResponse
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public SubjectInfo Subject { get; set; } = default!;
            public SemesterInfo Semester { get; set; } = default!;
        }

        private class SubjectInfo
        {
            public string Name { get; set; } = string.Empty;
        }

        private class SemesterInfo
        {
            public string Name { get; set; } = string.Empty;
        }

        private class ExamExaminerDto
        {
            public int ExaminerId { get; set; }
            public string ExaminerName { get; set; } = string.Empty;
            public string ExaminerEmail { get; set; } = string.Empty;
            public bool IsPrimaryGrader { get; set; }
            public DateTime AssignedAt { get; set; }
        }
    }
}
