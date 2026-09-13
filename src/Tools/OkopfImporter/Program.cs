using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence.Seed;

var csvPath = args.Length > 0 ? args[0] : "docs/data/okopf/okopf.csv";

// Используем тот же connection string, что и в фабрике дизайна
var cs = Environment.GetEnvironmentVariable("FIDUCIA_CS")
         ?? "Host=localhost;Port=5434;Database=fiducia;Username=fiducia;Password=fiducia";
var options = new DbContextOptionsBuilder<FiduciaDbContext>()
    .UseNpgsql(cs)
    .Options;

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?> { ["Edin:Enabled"] = "false" })
    .Build();
var loggerFactory = LoggerFactory.Create(builder => { });

await using var db = new FiduciaDbContext(options, configuration, loggerFactory);

Console.WriteLine($"Importing OKOPF from: {csvPath}");
await OkopfImport.RunAsync(db, csvPath);

Console.WriteLine("Done.");
