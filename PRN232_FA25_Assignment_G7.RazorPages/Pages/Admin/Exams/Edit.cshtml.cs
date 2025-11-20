using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Admin.Exams;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApiClient _apiClient;

    public EditModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public ExamInput ExamInput { get; set; } = new();

    public List<SelectListItem> Subjects { get; set; } = new();
    public List<SelectListItem> Semesters { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            var exam = await _apiClient.GetAsync<ExamDetail>($"/api/exams/{id}");
            if (exam == null) return NotFound();

            ExamInput = new ExamInput
            {
                Id = exam.Id,
                SubjectId = exam.Subject.Id,
                SemesterId = exam.Semester.Id,
                Name = exam.Name,
                Description = exam.Description,
                ExamDate = exam.ExamDate
            };

            await LoadDropdowns();
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return RedirectToPage("/Account/Login");
        }
        catch (ApiException)
        {
            TempData["Error"] = "Cannot reach API";
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return Page();
        }

        try
        {
            var request = new
            {
                ExamInput.SubjectId,
                ExamInput.SemesterId,
                ExamInput.Name,
                ExamInput.Description,
                ExamInput.ExamDate
            };

                await _apiClient.PutAsync<object, object>($"/api/exams/{ExamInput.Id}", request);
            TempData["Success"] = "Exam updated successfully";
            return RedirectToPage("./Index");
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Content ?? "Invalid data");
            await LoadDropdowns();
            return Page();
        }
        catch (ApiException)
        {
            TempData["Error"] = "Cannot reach API";
            await LoadDropdowns();
            return Page();
        }
    }

    private async Task LoadDropdowns()
    {
        var subjects = await _apiClient.GetAsync<List<SubjectItem>>("/api/subjects");
        var semesters = await _apiClient.GetAsync<List<SemesterItem>>("/api/semesters");

        Subjects = subjects?.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList() ?? new();
        Semesters = semesters?.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList() ?? new();
    }
}

public class ExamInput
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid SubjectId { get; set; }
    
    [Required]
    public Guid SemesterId { get; set; }
    
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public DateTime ExamDate { get; set; }
}

public class ExamDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExamDate { get; set; }
    public SubjectInfo Subject { get; set; } = new();
    public SemesterInfo Semester { get; set; } = new();
}

public class SubjectInfo
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class SemesterInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SubjectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SemesterItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
