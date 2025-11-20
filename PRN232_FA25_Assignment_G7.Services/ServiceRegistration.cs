using Microsoft.Extensions.DependencyInjection;
using PRN232_FA25_Assignment_G7.Services.Helpers;
using PRN232_FA25_Assignment_G7.Services.Implementations;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IRubricService, RubricService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IViolationService, ViolationService>();
        services.AddScoped<IExaminerService, ExaminerService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IBulkUploadService, BulkUploadService>();
        services.AddScoped<IModeratorService, ModeratorService>();
        services.AddScoped<IImportService, ImportService>();

        // Register helpers
        services.AddScoped<DuplicateChecker>();
        services.AddScoped<ViolationDetector>();
        services.AddScoped<FileProcessingHelper>();
        services.AddScoped<WordImageExtractor>();
        services.AddScoped<JwtTokenGenerator>();

        return services;
    }
}
