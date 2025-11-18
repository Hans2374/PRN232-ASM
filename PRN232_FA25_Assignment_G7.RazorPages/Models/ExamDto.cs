using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public class ExamResponse
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public Guid SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExamDate { get; set; }
}

public class ExamDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExamDate { get; set; }
    public SubjectResponse Subject { get; set; } = new();
    public SemesterResponse Semester { get; set; } = new();
    public List<RubricResponse> Rubrics { get; set; } = new();
    public int TotalSubmissions { get; set; }
}

public class CreateExamRequest
{
    [Required]
    [Display(Name = "Subject")]
    public Guid SubjectId { get; set; }

    [Required]
    [Display(Name = "Semester")]
    public Guid SemesterId { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Name must not exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Exam Date")]
    public DateTime ExamDate { get; set; } = DateTime.Today;
}

public class UpdateExamRequest
{
    [Required]
    [Display(Name = "Subject")]
    public Guid SubjectId { get; set; }

    [Required]
    [Display(Name = "Semester")]
    public Guid SemesterId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Exam Date")]
    public DateTime ExamDate { get; set; }
}

public class RubricResponse
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string Criteria { get; set; } = string.Empty;
    public int MaxScore { get; set; }
}
