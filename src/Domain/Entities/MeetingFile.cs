namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь заседания СД с файлами (meeting_files).
/// </summary>
public class MeetingFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на заседание (meeting_id).</summary>
    public Guid MeetingId { get; set; }

    /// <summary>Ссылка на файл (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Навигация к заседанию.</summary>
    public Meeting? Meeting { get; set; }

    /// <summary>Навигация к файлу.</summary>
    public FileEntry? File { get; set; }
}
