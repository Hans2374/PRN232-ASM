using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN232_FA25_Assignment_G7.Repositories.Interfaces;
using PRN232_FA25_Assignment_G7.Repositories.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Repositories.Interfaces;
using PRN232_FA25_Assignment_G7.Repositories.Repositories.Implementations;

namespace PRN232_FA25_Assignment_G7.Repositories.Extensions;

public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration, string? connectionStringName = null)
    {
        var name = connectionStringName ?? "DefaultConnection";
        var connectionString = configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure();
            });
        });

        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        // register other repositories here as added

        return services;
    }
}
