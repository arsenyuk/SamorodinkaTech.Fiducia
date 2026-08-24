namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Метаданные файла в едином файловом хранилище (ADR-020, BDR-011).
/// Соответствует таблице files.
/// </summary>
public class FileEntry
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Оригинальное имя файла (original_name).</summary>
    public string OriginalName { get; set; } = null!;

    /// <summary>MIME-тип (content_type).</summary>
    public string? ContentType { get; set; }

    /// <summary>Размер в байтах (size_bytes).</summary>
    public long SizeBytes { get; set; }

    /// <summary>Провайдер хранения (storage_provider): LOCAL или S3.</summary>
    public string StorageProvider { get; set; } = null!;

    /// <summary>Ключ или путь хранения (storage_key_or_path).</summary>
    public string StorageKeyOrPath { get; set; } = null!;

    /// <summary>Контрольная сумма SHA-256 (checksum).</summary>
    public string? Checksum { get; set; }

    /// <summary>Дата создания (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Создатель записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Тип файла в контексте использования (file_type). Например: CHARTER, PROTOCOL, RDL_EXTRACT.</summary>
    public string? FileType { get; set; }

    /// <summary>Пользовательское отображаемое имя (display_name).</summary>
    public string? DisplayName { get; set; }

    /// <summary>Расширение файла без точки (extension). Например: pdf, docx, png.</summary>
    public string? Extension { get; set; }

    /// <summary>Флаг завершения загрузки (is_uploaded). false пока чанки не собраны.</summary>
    public bool IsUploaded { get; set; } = true;

    /// <summary>Идентификатор сессии chunked upload (upload_id).</summary>
    public string? UploadId { get; set; }

    /// <summary>Время жизни незавершённой записи (expires_at).</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Сырой URL из QR-кода нотариального документа (qr_raw_url).</summary>
    public string? QrRawUrl { get; set; }

    /// <summary>Реестровый номер нотариального акта (qr_registry_number).</summary>
    public string? QrRegistryNumber { get; set; }

    /// <summary>ФИО нотариуса (qr_notary_full_name).</summary>
    public string? QrNotaryFullName { get; set; }

    /// <summary>Дата нотариального удостоверения (qr_notarization_date).</summary>
    public DateOnly? QrNotarizationDate { get; set; }

    /// <summary>Вид документа (qr_document_type).</summary>
    public string? QrDocumentType { get; set; }

    /// <summary>ФИО заявителя (qr_applicant_name).</summary>
    public string? QrApplicantName { get; set; }
}
