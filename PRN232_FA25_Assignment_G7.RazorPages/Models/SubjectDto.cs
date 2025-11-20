using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.RazorPages.Models;

public class SubjectResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateSubjectRequest
{
    [Required]
    [StringLength(20, ErrorMessage = "Code must not exceed 20 characters")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200, ErrorMessage = "Name must not exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
}

public class UpdateSubjectRequest
{
    [Required]
    [StringLength(20, ErrorMessage = "Code must not exceed 20 characters")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200, ErrorMessage = "Name must not exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
}
