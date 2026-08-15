using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services
{
    /// <summary>
    /// Сервис chunked upload: управление сессиями загрузки, запись чанков, сборка файлов (BDR-011).
    /// </summary>
    public sealed class ChunkedUploadService : IChunkedUploadService
    {
        private readonly ILogger<ChunkedUploadService> _logger;
        private readonly FileUploadOptions _options;
        private readonly IFileStorage _fileStorage;
        private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;

        public ChunkedUploadService(
            ILogger<ChunkedUploadService> logger,
            IOptions<FileUploadOptions> options,
            IFileStorage fileStorage,
            IDbContextFactory<FiduciaDbContext> dbFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        /// <inheritdoc/>
        public async Task<string> InitiateUploadAsync(string fileName, string? contentType, long totalSizeBytes, CancellationToken cancellationToken = default)
        {
            if (totalSizeBytes > _options.MaxFileSizeBytes)
                throw new InvalidOperationException($"Размер файла ({totalSizeBytes} байт) превышает максимальный({_options.MaxFileSizeBytes} байт).");

            // Проверка запрещённых расширений
            var extension = Path.GetExtension(fileName)?.TrimStart('.').ToLowerInvariant();
            if (!string.IsNullOrEmpty(extension) && _options.BlockedExtensions.Contains(extension))
                throw new InvalidOperationException($"Загрузка файлов с расширением .{extension} запрещена.");

            var uploadId = Guid.NewGuid().ToString("N");
            var tempDir = GetTempDir(uploadId);
            Directory.CreateDirectory(tempDir);

            await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var fileEntry = new FileEntry
            {
                Id = Guid.NewGuid(),
                OriginalName = fileName,
                ContentType = contentType,
                SizeBytes = totalSizeBytes,
                StorageProvider = "LOCAL",
                StorageKeyOrPath = $"pending/{uploadId}",
                IsUploaded = false,
                UploadId = uploadId,
                ExpiresAt = DateTime.UtcNow.AddHours(_options.UploadExpirationHours),
                Extension = Path.GetExtension(fileName)?.TrimStart('.')
            };
            ctx.Files.Add(fileEntry);
            await ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Инициирована загрузка: uploadId={UploadId}, file={FileName}, size={Size}",
                uploadId, fileName, totalSizeBytes);

            return uploadId;
        }

        /// <inheritdoc/>
        public async Task UploadChunkAsync(string uploadId, int chunkIndex, Stream chunkData, CancellationToken cancellationToken = default)
        {
            var tempDir = GetTempDir(uploadId);
            if (!Directory.Exists(tempDir))
                throw new InvalidOperationException($"Сессия загрузки {uploadId} не найдена.");

            var chunkPath = Path.Combine(tempDir, $"chunk_{chunkIndex:D8}");
            await using (var fs = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await chunkData.CopyToAsync(fs, cancellationToken);
            }

            _logger.LogDebug("Записан чанк {Index} для uploadId={UploadId}", chunkIndex, uploadId);
        }

        /// <inheritdoc/>
        public async Task<FileEntry> CompleteUploadAsync(string uploadId, CancellationToken cancellationToken = default)
        {
            var tempDir = GetTempDir(uploadId);
            if (!Directory.Exists(tempDir))
                throw new InvalidOperationException($"Сессия загрузки {uploadId} не найдена.");

            // Собираем все чанки в один поток
            var chunkFiles = Directory.GetFiles(tempDir, "chunk_*")
                .OrderBy(f => f)
                .ToList();

            if (chunkFiles.Count == 0)
                throw new InvalidOperationException($"Чанки не найдены для uploadId={uploadId}.");

            var assembledPath = Path.Combine(tempDir, "assembled");
            await using (var outputStream = new FileStream(assembledPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                foreach (var chunkFile in chunkFiles)
                {
                    await using var chunkStream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                    await chunkStream.CopyToAsync(outputStream, cancellationToken);
                }
            }

            // Вычисляем SHA-256
            string checksum;
            await using (var cs = File.OpenRead(assembledPath))
            {
                var hash = await SHA256.HashDataAsync(cs, cancellationToken);
                checksum = Convert.ToHexString(hash).ToLowerInvariant();
            }

            // Сохраняем в IFileStorage
            await using var storageStream = File.OpenRead(assembledPath);
            var ext = Path.GetExtension(await GetOriginalNameAsync(uploadId, cancellationToken));
            var storageKey = await _fileStorage.SaveAsync(storageStream, $"upload{ext}", null, cancellationToken);

            // Обновляем запись в БД
            await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var fileEntry = await ctx.Files.FirstOrDefaultAsync(f => f.UploadId == uploadId, cancellationToken)
                ?? throw new InvalidOperationException($"Запись файла для uploadId={uploadId} не найдена.");

            fileEntry.StorageKeyOrPath = storageKey;
            fileEntry.StorageProvider = storageKey.StartsWith('/') || Path.DirectorySeparatorChar == '/' && storageKey.Contains('/')
                ? "LOCAL" : "S3";
            fileEntry.Checksum = checksum;
            fileEntry.IsUploaded = true;
            fileEntry.UploadId = null;
            fileEntry.ExpiresAt = null;

            await ctx.SaveChangesAsync(cancellationToken);

            // Удаляем временную папку
            TryDeleteDirectory(tempDir);

            _logger.LogInformation("Загрузка завершена: uploadId={UploadId}, fileId={FileId}, storageKey={Key}",
                uploadId, fileEntry.Id, storageKey);

            return fileEntry;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<int>> GetUploadedChunksAsync(string uploadId, CancellationToken cancellationToken = default)
        {
            var tempDir = GetTempDir(uploadId);
            if (!Directory.Exists(tempDir))
                return Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());

            var chunks = Directory.GetFiles(tempDir, "chunk_*")
                .Select(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('_')[1]))
                .OrderBy(i => i)
                .ToList();

            return Task.FromResult<IReadOnlyList<int>>(chunks);
        }

        /// <inheritdoc/>
        public async Task AbortUploadAsync(string uploadId, CancellationToken cancellationToken = default)
        {
            // Удаляем запись из БД
            await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var fileEntry = await ctx.Files.FirstOrDefaultAsync(f => f.UploadId == uploadId, cancellationToken);
            if (fileEntry != null)
            {
                ctx.Files.Remove(fileEntry);
                await ctx.SaveChangesAsync(cancellationToken);
            }

            // Удаляем временную папку
            var tempDir = GetTempDir(uploadId);
            TryDeleteDirectory(tempDir);

            _logger.LogInformation("Загрузка отменена: uploadId={UploadId}", uploadId);
        }

        private string GetTempDir(string uploadId)
        {
            var basePath = _options.TempBasePath
                ?? Path.Combine(AppContext.BaseDirectory, "uploads");
            return Path.Combine(basePath, uploadId);
        }

        private async Task<string> GetOriginalNameAsync(string uploadId, CancellationToken cancellationToken)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entry = await ctx.Files.FirstOrDefaultAsync(f => f.UploadId == uploadId, cancellationToken);
            return entry?.OriginalName ?? "unknown";
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception)
            {
                // Игнорируем ошибки удаления временных файлов
            }
        }
    }
}
