namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Пункт структурированного требования участника (share_request_items).
/// Позволяет разбить требование на отдельные пункты с файлами к каждому.
/// </summary>
public class ShareRequestItem
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор требования (share_request_id).</summary>
    public Guid ShareRequestId { get; set; }

    /// <summary>Требование.</summary>
    public ShareRequest? ShareRequest { get; set; }

    /// <summary>Порядковый номер пункта (sequence_number).</summary>
    public int SequenceNumber { get; set; }

    /// <summary>Заголовок пункта (title).</summary>
    public string Title { get; set; } = default!;

    /// <summary>Описание пункта — текст требования (description).</summary>
    public string? Description { get; set; }

    /// <summary>Статус пункта: pending / approved / rejected (status).</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Формулировка отклонения (rejection_reason). Заполняется при status = rejected.</summary>
    public string? RejectionReason { get; set; }

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата и время обновления (updated_at).</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Прикреплённые файлы.</summary>
    public ICollection<ShareRequestItemFile> Files { get; set; } = new List<ShareRequestItemFile>();
}
