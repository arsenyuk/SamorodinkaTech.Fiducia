using System;

namespace SamorodinkaTech.Fiducia.Domain.Entities
{
    /// <summary>
    /// Метаданные файла в едином файловом хранилище (ADR-020).
    /// Соответствует таблице files.
    /// </summary>
    public class FileEntry
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = null!;
        public string? ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string StorageProvider { get; set; } = null!; // 'LOCAL' | 'S3'
        public string StorageKeyOrPath { get; set; } = null!;
        public string? Checksum { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}
