using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PRN232_FA25_Assignment_G7.Repositories;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        // Adjust for your environment
        optionsBuilder.UseSqlServer("Server=localhost;Database=PRN232_Assignment;Trusted_Connection=True;TrustServerCertificate=True");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
