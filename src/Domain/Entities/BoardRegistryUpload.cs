namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Акт загрузки реестра участников — XML-файл и отсоединённая подпись (board_registry_upload).
/// </summary>
public class BoardRegistryUpload
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Идентификатор XML-файла в таблице files (xml_file_id).</summary>
    public Guid? XmlFileId { get; set; }

    /// <summary>Идентификатор файла подписи в таблице files (signature_file_id).</summary>
    public Guid? SignatureFileId { get; set; }

    /// <summary>Исходное имя XML-файла (xml_original_name).</summary>
    public string? XmlOriginalName { get; set; }

    /// <summary>Исходное имя файла подписи (signature_original_name).</summary>
    public string? SignatureOriginalName { get; set; }

    /// <summary>Статус загрузки: uploaded / processed / error (status).</summary>
    public string Status { get; set; } = "uploaded";

    /// <summary>Количество участников в файле (participant_count).</summary>
    public int? ParticipantCount { get; set; }

    /// <summary>Идентификатор пользователя, загрузившего файл (uploaded_by).</summary>
    public Guid? UploadedBy { get; set; }

    /// <summary>Дата загрузки (uploaded_at).</summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Время последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
