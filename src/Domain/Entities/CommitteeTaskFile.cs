namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь задачи комитета с файлами (committee_task_files).
/// </summary>
public class CommitteeTaskFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на задачу комитета (committee_task_id).</summary>
    public Guid CommitteeTaskId { get; set; }

    /// <summary>Ссылка на файл (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Навигация к задаче комитета.</summary>
    public CommitteeTask? CommitteeTask { get; set; }

    /// <summary>Навигация к файлу.</summary>
    public FileEntry? File { get; set; }
}
