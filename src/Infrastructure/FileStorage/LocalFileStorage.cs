using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.FileStorage
{
    /// <summary>
    /// Локальное файловое хранилище: сохраняет файлы в подкаталог от конфигурированного BasePath.
    /// Не управляет правами файлов — WORM-семантика обеспечивается на уровне инфраструктуры (umask, SGID, chattr, СХД).
    /// </summary>
    public sealed class LocalFileStorage : IFileStorage
    {
        private readonly ILogger<LocalFileStorage> _logger;
        private readonly string _basePath;

        public LocalFileStorage(IOptions<FileStorageOptions> options, ILogger<LocalFileStorage> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var opt = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _basePath = string.IsNullOrWhiteSpace(opt.LocalBasePath)
                ? Path.Combine(AppContext.BaseDirectory, "storage")
                : opt.LocalBasePath!;

            // Создаём каталог, если отсутствует
            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
        {
            // Генерируем подкаталоги по дате для снижения числа файлов в одном каталоге
            var now = DateTime.UtcNow;
            var relDir = Path.Combine(now.ToString("yyyy"), now.ToString("MM"), now.ToString("dd"));
            var targetDir = Path.Combine(_basePath, relDir);
            Directory.CreateDirectory(targetDir);

            // Уникальное имя файла: GUID + оригинальное расширение
            var ext = Path.GetExtension(fileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(targetDir, name);

            // Копируем поток на диск с вычислением SHA256 параллельно
            using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await content.CopyToAsync(fs, cancellationToken);
            }

            var storageKey = Path.Combine(relDir, name).Replace('\\', '/');
            _logger.LogInformation("Saved file to local storage: {Key}", storageKey);
            return storageKey;
        }

        public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_basePath, storageKey.Replace('/', Path.DirectorySeparatorChar));
            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            return Task.FromResult(stream);
        }

        public Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_basePath, storageKey.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath)) return Task.FromResult(false);
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
    }
}
