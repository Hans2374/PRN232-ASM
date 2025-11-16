using PRN232_FA25_Assignment_G7.Repositories.Extensions;
using PRN232_FA25_Assignment_G7.Services;

namespace PRN232_FA25_Assignment_G7.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Repositories
        services.AddRepositories(configuration);

        // Register Services
        services.AddServices();

        // AutoMapper
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }
}
