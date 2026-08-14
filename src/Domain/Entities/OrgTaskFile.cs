namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь задачи оргплана с файлами (org_task_files).
/// </summary>
public class OrgTaskFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на задачу оргплана (org_task_id).</summary>
    public Guid OrgTaskId { get; set; }

    /// <summary>Ссылка на файл (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Навигация к задаче оргплана.</summary>
    public OrgTask? OrgTask { get; set; }

    /// <summary>Навигация к файлу.</summary>
    public FileEntry? File { get; set; }
}
