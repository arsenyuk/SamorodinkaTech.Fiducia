using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces
{
    /// <summary>
    /// Сервис chunked upload: управление сессиями загрузки, запись чанков, сборка файлов.
    /// </summary>
    public interface IChunkedUploadService
    {
        /// <summary>
        /// Создаёт сессию загрузки и возвращает uploadId.
        /// </summary>
        /// <param name="fileName">Оригинальное имя файла.</param>
        /// <param name="contentType">MIME тип файла.</param>
        /// <param name="totalSizeBytes">Общий размер файла в байтах.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task<string> InitiateUploadAsync(string fileName, string? contentType, long totalSizeBytes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Записывает чанк по индексу.
        /// </summary>
        /// <param name="uploadId">Идентификатор сессии загрузки.</param>
        /// <param name="chunkIndex">Индекс чанка (начиная с 0).</param>
        /// <param name="chunkData">Поток данных чанка.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task UploadChunkAsync(string uploadId, int chunkIndex, Stream chunkData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Собирает файл из чанков, сохраняет в IFileStorage, создаёт запись в files.
        /// </summary>
        /// <param name="uploadId">Идентификатор сессии загрузки.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Созданная запись FileEntry.</returns>
        Task<FileEntry> CompleteUploadAsync(string uploadId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает список загруженных чанков (для resume).
        /// </summary>
        /// <param name="uploadId">Идентификатор сессии загрузки.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task<IReadOnlyList<int>> GetUploadedChunksAsync(string uploadId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Откатывает незавершённую загрузку: удаляет временные чанки и запись из БД.
        /// </summary>
        /// <param name="uploadId">Идентификатор сессии загрузки.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task AbortUploadAsync(string uploadId, CancellationToken cancellationToken = default);
    }
}
