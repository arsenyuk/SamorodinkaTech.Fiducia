namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь пункта требования с файлами (share_request_item_files).
/// </summary>
public class ShareRequestItemFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор пункта требования (share_request_item_id).</summary>
    public Guid ShareRequestItemId { get; set; }

    /// <summary>Пункт требования.</summary>
    public ShareRequestItem? ShareRequestItem { get; set; }

    /// <summary>Идентификатор файла (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Файл.</summary>
    public FileEntry? File { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
