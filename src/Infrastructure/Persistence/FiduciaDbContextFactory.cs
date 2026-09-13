using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SamorodinkaTech.Fiducia.Infrastructure.Persistence;

public class FiduciaDbContextFactory : IDesignTimeDbContextFactory<FiduciaDbContext>
{
    public FiduciaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FiduciaDbContext>();
        // Dev default matches docker-compose (port 5434, user/password fiducia)
        var cs = Environment.GetEnvironmentVariable("FIDUCIA_CS")
                 ?? "Host=localhost;Port=5434;Database=fiducia;Username=fiducia;Password=fiducia";
        optionsBuilder.UseNpgsql(cs);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Edin:Enabled"] = "false" })
            .Build();
        var loggerFactory = LoggerFactory.Create(builder => { });

        return new FiduciaDbContext(optionsBuilder.Options, configuration, loggerFactory);
    }
}
