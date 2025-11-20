using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PRN232_FA25_Assignment_G7.Repositories;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Prefer an environment variable for connection when running design-time tools.
        // Fallback to the same connection used by the API appsettings to ensure migrations
        // are applied against the same database during development.
        var envConn = Environment.GetEnvironmentVariable("PRN232_CONNECTION_STRING");
        var connection = !string.IsNullOrEmpty(envConn)
            ? envConn
            : "Server=DESKTOP-2SQIHP7;Uid=sa;Pwd=12345;Database=PRN232_Assignment;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=True;";

        optionsBuilder.UseSqlServer(connection);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
