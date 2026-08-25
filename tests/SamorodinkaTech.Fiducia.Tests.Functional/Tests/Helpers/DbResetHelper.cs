using System.Diagnostics;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Сброс базы данных через утилиту DbReset.
/// </summary>
public static class DbResetHelper
{
    private static readonly string RepoRoot = FindRepoRoot();

    /// <summary>
    /// Сбросить БД. Если includeDemo = false — пропускает 03_demo.sql.
    /// </summary>
    public static async Task ResetAsync(bool includeDemo = false, TimeSpan? timeout = null)
    {
        var toolPath = Path.Combine(RepoRoot, "src", "Tools", "DbReset");
        var args = includeDemo ? "" : "--no-demo";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{toolPath}\" -- {args}",
                WorkingDirectory = RepoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.Start();

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(2);
        using var cts = new CancellationTokenSource(effectiveTimeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill();
            throw new TimeoutException($"DbReset timed out after {effectiveTimeout.TotalMinutes} minutes.");
        }

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"DbReset failed (exit {process.ExitCode}).\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 15 && dir != null; i++)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SamorodinkaTech.Fiducia.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return Environment.GetEnvironmentVariable("FIDUCIA_REPO")
               ?? Directory.GetCurrentDirectory();
    }
}
