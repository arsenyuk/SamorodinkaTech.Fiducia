namespace SamorodinkaTech.Fiducia.Domain.Models.Email;

/// <summary>
/// Вложение email-письма.
/// </summary>
public class EmailAttachment
{
    /// <summary>Содержимое вложения.</summary>
    public Stream Content { get; init; } = Stream.Null;

    /// <summary>Имя файла.</summary>
    public string FileName { get; init; } = "";

    /// <summary>MIME-тип содержимого.</summary>
    public string ContentType { get; init; } = "application/octet-stream";
}
