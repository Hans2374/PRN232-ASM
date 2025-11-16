using System.ComponentModel.DataAnnotations;

namespace PRN232_FA25_Assignment_G7.API.DTOs;

public record CreateSubjectRequest(
    [Required, StringLength(20)] string Code,
    [Required, StringLength(200)] string Name
);

public record UpdateSubjectRequest(
    [Required, StringLength(20)] string Code,
    [Required, StringLength(200)] string Name
);

public record SubjectResponse(
    Guid Id,
    string Code,
    string Name
);
