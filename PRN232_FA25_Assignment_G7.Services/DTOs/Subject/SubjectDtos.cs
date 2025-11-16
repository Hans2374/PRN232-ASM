namespace PRN232_FA25_Assignment_G7.Services.DTOs.Subject;

public record CreateSubjectRequest(string Code, string Name);
public record UpdateSubjectRequest(string Code, string Name);
public record SubjectResponse(Guid Id, string Code, string Name);
