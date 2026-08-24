namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Результат сканирования QR-кода нотариального документа (file_notarization).
/// Создаётся автоматически при загрузке файла, если QR-код распознан.
/// Связь 1:1 с файлом (file_id).
/// </summary>
public class FileNotarization
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор файла (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Сырой URL из QR-кода (raw_url).</summary>
    public string? RawUrl { get; set; }

    /// <summary>Реестровый номер нотариального акта (registry_number).</summary>
    public string? RegistryNumber { get; set; }

    /// <summary>ФИО нотариуса (notary_full_name).</summary>
    public string? NotaryFullName { get; set; }

    /// <summary>Дата нотариального удостоверения (notarization_date).</summary>
    public DateOnly? NotarizationDate { get; set; }

    /// <summary>Вид документа (document_type).</summary>
    public string? DocumentType { get; set; }

    /// <summary>ФИО заявителя (applicant_name).</summary>
    public string? ApplicantName { get; set; }

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Связь с файлом.</summary>
    public FileEntry? File { get; set; }
}
