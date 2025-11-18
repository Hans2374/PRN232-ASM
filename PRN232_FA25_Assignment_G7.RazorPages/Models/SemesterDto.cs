using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public class SemesterResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CreateSemesterRequest
{
    [Required]
    [StringLength(100, ErrorMessage = "Name must not exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(4);
}
