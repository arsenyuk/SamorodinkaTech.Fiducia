using Npgsql;
using System.Text;

// Simple DB reset tool: drop and recreate fiducia schema from tools/db SQLs

var cs = Environment.GetEnvironmentVariable("FIDUCIA_CS")
         ?? "Host=localhost;Port=5434;Database=fiducia;Username=fiducia;Password=fiducia";

await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();

// Danger note: dev-only. Drop all user tables in public schema
async Task ExecuteAsync(string sql)
{
    await using var cmd = new NpgsqlCommand(sql, conn);
    await cmd.ExecuteNonQueryAsync();
}

// Disable FK, drop tables if exist
var dropSql = @"
DO $$ DECLARE r RECORD;
BEGIN
  FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
    EXECUTE 'DROP TABLE IF EXISTS ' || quote_ident(r.tablename) || ' CASCADE';
  END LOOP;
END $$;";
await ExecuteAsync(dropSql);

// Apply schema then seed then demo
static string ReadFile(string path) => File.ReadAllText(path, Encoding.UTF8);

// Discover repo root by env or walking up until we see tools/db
string? FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    for (int i = 0; i < 10 && dir != null; i++)
    {
        var candidate = Path.Combine(dir.FullName, "tools", "db");
        if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "01_schema.sql")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return null;
}

// Prefer current working directory (repo root if run from it), else probe upwards
var repoRoot = Environment.GetEnvironmentVariable("FIDUCIA_REPO") ?? Directory.GetCurrentDirectory();
if (!Directory.Exists(Path.Combine(repoRoot, "tools", "db")))
    repoRoot = FindRepoRoot() ?? throw new DirectoryNotFoundException("Cannot locate repo root (tools/db not found)");
string RepoPath(params string[] parts) => Path.Combine(new[] { repoRoot }.Concat(parts).ToArray());

var schemaPath = RepoPath("tools", "db", "01_schema.sql");
var seedPath   = RepoPath("tools", "db", "02_seed.sql");
var demoPath   = RepoPath("tools", "db", "03_demo.sql");

Console.WriteLine($"Repo root: {repoRoot}");
Console.WriteLine($"Schema path: {schemaPath}");

var schemaSql = ReadFile(schemaPath);
var seedSql   = File.Exists(seedPath) ? ReadFile(seedPath) : string.Empty;
var demoSql   = File.Exists(demoPath) ? ReadFile(demoPath) : string.Empty;

await ExecuteAsync(schemaSql);
if (!string.IsNullOrWhiteSpace(seedSql)) await ExecuteAsync(seedSql);
if (!string.IsNullOrWhiteSpace(demoSql)) await ExecuteAsync(demoSql);

Console.WriteLine("Database reset complete.");
