namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь требования участника с файлами (share_request_files).
/// </summary>
public class ShareRequestFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на требование (share_request_id).</summary>
    public Guid ShareRequestId { get; set; }

    /// <summary>Ссылка на файл (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Навигация к требованию.</summary>
    public ShareRequest? ShareRequest { get; set; }

    /// <summary>Навигация к файлу.</summary>
    public FileEntry? File { get; set; }
}
