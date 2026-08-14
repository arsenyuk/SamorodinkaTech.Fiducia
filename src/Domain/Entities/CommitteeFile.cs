namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь комитета с файлами (committee_files).
/// </summary>
public class CommitteeFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на комитет (committee_id).</summary>
    public Guid CommitteeId { get; set; }

    /// <summary>Ссылка на файл (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Навигация к комитету.</summary>
    public Committee? Committee { get; set; }

    /// <summary>Навигация к файлу.</summary>
    public FileEntry? File { get; set; }
}
