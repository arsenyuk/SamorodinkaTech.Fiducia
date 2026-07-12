namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь ОСА с файлами (osa_meeting_files).
/// </summary>
public class OsaMeetingFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на ОСА (osa_meeting_id).</summary>
    public Guid OsaMeetingId { get; set; }

    /// <summary>Ссылка на файл (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Тип файла (file_type).</summary>
    public string FileType { get; set; } = default!;

    /// <summary>Пользовательское наименование (display_name).</summary>
    public string? DisplayName { get; set; }

    public OsaMeeting? OsaMeeting { get; set; }
    public FileEntry? File { get; set; }
}
