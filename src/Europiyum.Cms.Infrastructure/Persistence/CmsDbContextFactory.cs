using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Europiyum.Cms.Infrastructure.Persistence;

/// <summary>
/// Allows <c>dotnet ef</c> to create the context without starting the web host. Override connection string via env <c>CMS_DATABASE</c> if needed.
/// </summary>
public class CmsDbContextFactory : IDesignTimeDbContextFactory<CmsDbContext>
{
    public CmsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CMS_DATABASE")
            ?? "Host=localhost;Port=5432;Database=europiyum;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<CmsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new CmsDbContext(optionsBuilder.Options);
    }
}
