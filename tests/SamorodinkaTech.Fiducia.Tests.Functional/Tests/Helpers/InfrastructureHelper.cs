using System.Diagnostics;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для запуска и проверки инфраструктуры E2E-тестов:
/// PostgreSQL, OpenLDAP, phpLDAPadmin, Admin Console, Board Portal.
/// </summary>
public static class InfrastructureHelper
{
    private static bool _initialized;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private static readonly string ProjectRoot = FindProjectRoot();

    /// <summary>
    /// Найти корень проекта (директорию с .sln/.slnx файлом).
    /// </summary>
    private static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0 ||
                Directory.GetFiles(dir, "*.slnx").Length > 0)
                return dir;

            dir = Directory.GetParent(dir)?.FullName;
        }

        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Убедиться, что вся инфраструктура запущена.
    /// Вызывать один раз перед прогоном E2E-тестов.
    /// </summary>
    public static async Task EnsureInfrastructureReadyAsync()
    {
        if (_initialized) return;

        await Semaphore.WaitAsync();
        try
        {
            if (_initialized) return;

            Console.WriteLine("[Infra] Запуск инфраструктуры...");
            await StartInfrastructureAsync();
            Console.WriteLine("[Infra] Инфраструктура запущена, ожидание готовности...");
            await WaitForServicesAsync();
            Console.WriteLine("[Infra] Инфраструктура готова.");
            _initialized = true;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Перезапустить порталы (Admin Console + Board Portal).
    /// Нужно после DbReset, чтобы DbContext пересоздался с новыми данными.
    /// Использует start.sh для запуска.
    /// </summary>
    public static async Task RestartPortalsAsync()
    {
        Console.WriteLine("[Infra] Остановка порталов...");
        await RunCommandAsync("pkill", "-f 'dotnet.*AdminConsole'", timeout: TimeSpan.FromSeconds(5));
        await RunCommandAsync("pkill", "-f 'dotnet.*BoardPortal'", timeout: TimeSpan.FromSeconds(5));
        await Task.Delay(3000);

        Console.WriteLine("[Infra] Запуск порталов через start.sh...");
        await RunCommandAsync("bash", "./start.sh",
            timeout: TimeSpan.FromSeconds(30));

        await WaitForServicesAsync();
        Console.WriteLine("[Infra] Порталы перезапущены.");
    }

    /// <summary>
    /// Запустить инфраструктуру через docker-compose и dotnet run.
    /// </summary>
    private static async Task StartInfrastructureAsync()
    {
        // ═══════════════════════════════════════════════════════════════════
        // Шаг 1: PostgreSQL + OpenLDAP + phpLDAPadmin через docker-compose
        // ═══════════════════════════════════════════════════════════════════
        await RunCommandAsync("docker-compose", "up -d postgres", timeout: TimeSpan.FromMinutes(2));
        await RunCommandAsync("docker-compose", "-f docker-compose.ldap.yml up -d", timeout: TimeSpan.FromMinutes(2));

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 2: Admin Console (порт 5001)
        // ═══════════════════════════════════════════════════════════════════
        if (!await IsPortOpenAsync(5001))
        {
            await RunCommandAsync("dotnet", "run --project SamorodinkaTech.Fiducia.AdminConsole",
                timeout: TimeSpan.FromMinutes(1), background: true);
        }

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 3: Board Portal (порт 5002)
        // ═══════════════════════════════════════════════════════════════════
        if (!await IsPortOpenAsync(5002))
        {
            await RunCommandAsync("dotnet", "run --project SamorodinkaTech.Fiducia.BoardPortal",
                timeout: TimeSpan.FromMinutes(1), background: true);
        }
    }

    /// <summary>
    /// Дождаться готовности всех сервисов.
    /// </summary>
    private static async Task WaitForServicesAsync()
    {
        var services = new (string Name, string Url)[]
        {
            ("PostgreSQL", "http://localhost:5001"),
            ("phpLDAPadmin", "http://localhost:8082"),
            ("Admin Console", "http://localhost:5001"),
            ("Board Portal", "http://localhost:5002")
        };

        foreach (var (name, url) in services)
        {
            await WaitForServiceAsync(name, url, timeout: TimeSpan.FromMinutes(2));
        }
    }

    /// <summary>
    /// Дождаться доступности сервиса по URL.
    /// </summary>
    private static async Task WaitForServiceAsync(string name, string url, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        while (sw.Elapsed < timeout)
        {
            try
            {
                var response = await client.GetAsync(url);
                if ((int)response.StatusCode < 500)
                {
                    Console.WriteLine($"  ✓ {name} готов ({(int)response.StatusCode})");
                    return;
                }
            }
            catch
            {
                // Сервис ещё не готов
            }

            await Task.Delay(2000);
        }

        Console.WriteLine($"  ⚠ {name} не ответил за {timeout.TotalSeconds}с, продолжаем...");
    }

    /// <summary>
    /// Проверить, открыт ли порт.
    /// </summary>
    private static async Task<bool> IsPortOpenAsync(int port)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"http://localhost:{port}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Выполнить shell-команду из корня проекта.
    /// </summary>
    private static async Task RunCommandAsync(string command, string arguments,
        TimeSpan? timeout = null, bool background = false)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = ProjectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process is null)
            throw new InvalidOperationException($"Не удалось запустить процесс: {command} {arguments}");

        if (background)
        {
            // Для фоновых процессов не ждём завершения
            Console.WriteLine($"  ▸ Фоновый процесс: {command} {arguments} (PID: {process.Id})");
            return;
        }

        var ct = new CancellationTokenSource(timeout ?? TimeSpan.FromMinutes(1));
        try
        {
            await process.WaitForExitAsync(ct.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill();
            Console.WriteLine($"  ⚠ Таймаут: {command} {arguments}");
        }

        if (process.ExitCode != 0)
        {
            var stderr = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                Console.WriteLine($"  ⚠ {command} завершился с кодом {process.ExitCode}: {stderr[..Math.Min(200, stderr.Length)]}");
            }
        }
    }
}
