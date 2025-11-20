using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN232_FA25_Assignment_G7.RazorPages.Services;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Pages.Examiner.Submissions;

[Authorize(Roles = "Examiner")]
public class GradeModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ILogger<GradeModel> _logger;

    public GradeModel(ApiClient apiClient, AuthSession authSession, ILogger<GradeModel> logger)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _logger = logger;
    }

    [BindProperty]
    public SubmitGradeRequest? GradeRequest { get; set; }

    public SubmissionDetailDto? Submission { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (!_authSession.IsInRole(HttpContext, "Examiner"))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            Submission = await _apiClient.GetExaminerSubmissionDetailAsync(id);

            if (Submission == null)
            {
                ErrorMessage = "Submission not found.";
                return Page();
            }

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized access to grade submission {SubmissionId}", id);
            return RedirectToPage("/Account/Login");
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            ErrorMessage = "Submission not found.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading submission {SubmissionId} for grading", id);
            ErrorMessage = "Unable to load submission. Please try again later.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!_authSession.IsInRole(HttpContext, "Examiner"))
        {
            return RedirectToPage("/Account/Login");
        }

        if (GradeRequest == null)
        {
            ErrorMessage = "Invalid grading data.";
            return await OnGetAsync(id);
        }

        try
        {
            var success = await _apiClient.SubmitExaminerGradeAsync(id, GradeRequest);

            if (success)
            {
                TempData["Success"] = "Grade submitted successfully!";
                return RedirectToPage("Index");
            }
            else
            {
                ErrorMessage = "Failed to submit grade. Please try again.";
                return await OnGetAsync(id);
            }
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized attempt to submit grade for submission {SubmissionId}", id);
            return RedirectToPage("/Account/Login");
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            ErrorMessage = ex.Message ?? "Invalid grading data.";
            return await OnGetAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting grade for submission {SubmissionId}", id);
            ErrorMessage = "Unable to submit grade. Please try again later.";
            return await OnGetAsync(id);
        }
    }
}

