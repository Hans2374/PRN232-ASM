using AutoMapper;
using PRN232_FA25_Assignment_G7.API.DTOs;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Mapping;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // Subject mappings
        CreateMap<Subject, SubjectResponse>();
        CreateMap<CreateSubjectRequest, Subject>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Semester mappings
        CreateMap<Semester, SemesterResponse>();
        CreateMap<CreateSemesterRequest, Semester>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Exam mappings
        CreateMap<Exam, ExamResponse>()
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : string.Empty))
            .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.Name : string.Empty));

        CreateMap<Exam, ExamDetailResponse>()
            .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
            .ForMember(dest => dest.Semester, opt => opt.MapFrom(src => src.Semester))
            .ForMember(dest => dest.Rubrics, opt => opt.MapFrom(src => src.Rubrics))
            .ForMember(dest => dest.TotalSubmissions, opt => opt.MapFrom(src => src.Submissions.Count));

        // Rubric mappings
        CreateMap<Rubric, RubricResponse>();
        CreateMap<AddRubricRequest, Rubric>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Submission mappings
        CreateMap<Submission, SubmissionResponse>()
            .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam != null ? src.Exam.Name : string.Empty))
            .ForMember(dest => dest.ViolationCount, opt => opt.MapFrom(src => src.Violations.Count));

        // Violation mappings
        CreateMap<Violation, ViolationResponse>()
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Submission != null ? src.Submission.StudentCode : string.Empty));

        // Examiner mappings
        CreateMap<Examiner, ExaminerResponse>()
            .ForMember(dest => dest.AssignedSubjects, opt => opt.MapFrom(src => src.ExaminerSubjects.Select(es => es.Subject != null ? es.Subject.Name : string.Empty).ToList()));
    }
}
